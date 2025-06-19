using BloodDonationAPI.DTO;
using BloodDonationAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace BloodDonationAPI.Service
{
    public class AppointmentServiece : IAppointmentServiece
    {
        private readonly BloodDonationSystemContext _context;
        public AppointmentServiece(BloodDonationSystemContext context)
        {
            _context = context;
        }
        public async Task<List<EventDto>> GetEventsLists()
        {
            var today = DateOnly.FromDateTime(DateTime.Now);

            return await _context.Events
                .Where(a => a.EventDate >= today)
                .Select(a => new EventDto
                {
                    EventId = a.EventId,
                    EventTitle = a.EventTitle,
                    EventContent = a.EventContent,
                    EventDate = a.EventDate,
                    EventTime = a.EventTime,
                    Location = a.Location,
                    MaxParticipants = a.MaxParticipants
                })
                .ToListAsync();

        }
        public async Task<string> RegisterAppointment(string userName , RegisterAppointmentDto Dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == userName);
            if (user == null)
                return "User not found.";

            if (user.ProfileStatus != "Active")
                return "Bạn chưa đủ điều kiện để đăng ký.";

            var appointment = await _context.Events
                .FirstOrDefaultAsync(a => a.EventId == Dto.eventId);

            if (appointment == null)
                return "Lịch hẹn không tồn tại.";

            bool alreadyRegistered = await _context.AppointmentRecords.AnyAsync(h =>
                h.Username == userName && h.EventId == Dto.eventId);

            if (alreadyRegistered)
                return "Bạn đã đăng ký lịch hẹn này rồi.";

            var history = new AppointmentRecord
            {
                Username = userName,
                EventId = Dto.eventId,
                RegistrationDate = DateTime.Now,
                Status = "Đã đăng ký"
            };

                _context.AppointmentRecords.Add(history);
                await _context.SaveChangesAsync();

                return "Bạn đã đăng ký thành công lịch hẹn";
        }
        public async Task<List<AppointmentHistoryDto>> GetByUsernameAsync(string username)
        {
            var records = await _context.AppointmentRecords
                .Where(h => h.Username == username)
                .Include(h => h.Event)
                .Include(h => h.BloodDetails)
                    .ThenInclude(b => b.Hospital)
                .OrderByDescending(h => h.RegistrationDate)
                .ToListAsync();

            return records.Select(h => new AppointmentHistoryDto
            {
                AppointmentId = h.AppointmentId,
                EventId = h.EventId,
                AppointmentDate = h.RegistrationDate,
                AppointmentStatus = h.Status,

                // Thông tin lịch hẹn
                AppointmentDateOfAppointment = h.Event?.EventDate,
                AppointmentTime = h.Event?.EventTime,
                AppointmentTitle = h.Event?.EventTitle,
                AppointmentContent = h.Event?.EventContent,

                // Thông tin hiến máu (nếu có)
                BloodType = h.BloodType,
                DonationUnit = h.DonationUnit,
                BloodStatus = h.BloodDetails.FirstOrDefault()?.BloodDetailStatus,
                BloodLocation = h.BloodDetails.FirstOrDefault()?.Hospital?.HospitalName
            }).ToList();

        }

        public async Task<bool> CancelAppointmentAsync(int appointmentId)
        {
            var appoinment = await _context.AppointmentRecords.FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

            if (appoinment == null || appoinment.Status=="Hủy")
            {
                return false; // Lịch hẹn không tồn tại
            }

            appoinment.Status = "Hủy"; // Cập nhật trạng thái lịch hẹn
            await _context.SaveChangesAsync(); // Lưu thay đổi vào cơ sở dữ liệu
            return true; // Trả về true nếu cập nhật thành công
        }
    }
}
