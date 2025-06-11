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
        public async Task<List<AppointmentList>> GetAppointmentLists()
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            return await _context.AppointmentLists
                .Where(a => a.AppointmentDate >= today)
                .ToListAsync();

        }
        public async Task<string> RegisterAppointment(string userName , RegisterAppointmentDto Dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == userName);
            if (user == null)
                return "User not found.";

            if (user.ProfileStatus != "Active")
                return "Bạn chưa đủ điều kiện để đăng ký.";

            var appointment = await _context.AppointmentLists
                .FirstOrDefaultAsync(a => a.AppointmentId == Dto.appointmentId);

            if (appointment == null)
                return "Lịch hẹn không tồn tại.";

            bool alreadyRegistered = await _context.AppointmentHistories.AnyAsync(h =>
                h.Username == userName && h.AppointmentId == Dto.appointmentId);

            if (alreadyRegistered)
                return "Bạn đã đăng ký lịch hẹn này rồi.";

            var history = new AppointmentHistory
            {
                Username = userName,
                AppointmentId = Dto.appointmentId,
                AppointmentDate = DateTime.Now,
                AppointmentStatus = "registered"
            };

                _context.AppointmentHistories.Add(history);
                await _context.SaveChangesAsync();

                return "Success";
        }
        public async Task<List<AppointmentHistoryDto>> GetByUsernameAsync(string username)
        {
            return await _context.AppointmentHistories
                .Where(h => h.Username == username)
                .Include(h => h.Appointment)
                .OrderByDescending(h => h.AppointmentDate)
                .Select(h => new AppointmentHistoryDto
                {
                    AppointmentHistoryId = h.AppointmentHistoryId,
                    AppointmentId = h.AppointmentId, 
                    AppointmentDate = h.AppointmentDate,
                    AppointmentStatus = h.AppointmentStatus,
                    AppointmentDateOfAppointment = h.Appointment != null ? h.Appointment.AppointmentDate : null,
                    AppointmentTime = h.Appointment != null ? h.Appointment.AppointmentTime : null,
                    AppointmentTitle = h.Appointment != null ? h.Appointment.AppointmentTitle : null,
                    AppointmentContent = h.Appointment != null ? h.Appointment.AppointmentContent : null,
                })
                .ToListAsync();
        }

        public async Task<bool> CancelAppointmentAsync(int appointmentId)
        {
            var appoinment = await _context.AppointmentHistories.FirstOrDefaultAsync(a => a.AppointmentHistoryId == appointmentId);

            if (appoinment == null || appoinment.AppointmentStatus=="Canceled")
            {
                return false; // Lịch hẹn không tồn tại
            }

            appoinment.AppointmentStatus = "Canceled"; // Cập nhật trạng thái lịch hẹn
            await _context.SaveChangesAsync(); // Lưu thay đổi vào cơ sở dữ liệu
            return true; // Trả về true nếu cập nhật thành công
        }
    }
}
