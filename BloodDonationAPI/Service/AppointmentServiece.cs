using BloodDonationAPI.DTO;
using BloodDonationAPI.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

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
            //kiểm tra người dùng có tồn tại và đủ điều kiện đăng ký không
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == userName);
            if (user == null)
                return "User not found.";

            if (user.ProfileStatus != "Active")
                return "Bạn chưa đủ điều kiện để đăng ký.";
            //kiểm tra lịch hẹn có tồn tại không
            var appointment = await _context.Events
                .FirstOrDefaultAsync(a => a.EventId == Dto.eventId);

            if (appointment == null)
                return "Lịch hẹn không tồn tại.";
            //kiểm tra xem đã đăng ký lịch hẹn này chưa
            bool alreadyRegistered = await _context.AppointmentRecords.AnyAsync(h =>
                h.Username == userName && h.EventId == Dto.eventId && h.Status != "Hủy");

            if (alreadyRegistered)
                return "Bạn đã đăng ký lịch hẹn này rồi.";
            // nếu chưa có lịch hẹn thì thêm mới vào bảng AppointmentRecords
            var history = new AppointmentRecord
            {
                Username = userName,
                EventId = Dto.eventId,
                RegistrationDate = DateTime.Now,
                Status = "Đang xét duyệt"
            };

                _context.AppointmentRecords.Add(history);
                await _context.SaveChangesAsync();
            return "Bạn đã đăng ký thành công lịch hẹn";

            //var appointmentId = history.AppointmentId;

            //// Kiểm tra xem người dùng đã trả lời khảo sát chưa
            //bool hasAnswers = await _context.UserSurveyAnswers
            //    .AnyAsync(a => a.AppointmentId == appointmentId);

            //if (!hasAnswers)
            //{
            //    return "Bạn đã đăng ký thành công lịch hẹn. Vui lòng trả lời khảo sát để xác định đủ điều kiện hiến máu.";
            //}
            ////kiêm tra xem người dùng có đủ điều kiện hiến máu chưa

            //var status = "Đang xét duyệt";//satus mặc định là đang xét duyệt neu can xem xet them 

            //bool? isEligible = await CheckUserAnsweredSurvey(appointmentId);
            //if (isEligible == true)
            //{
            //    // Cập nhật thông tin hiến máu nếu đủ điều kiện
            //    status = "Đã Đăng ký";

            //}
            //else if (isEligible == false)
            //{
            //    // Cập nhật thông tin hiến máu nếu không đủ điều kiện
            //    status = "Không đủ điều kiện";
            //}
            //// Cập nhật trạng thái lịch hẹn
            //history.Status = status;
            //_context.AppointmentRecords.Update(history);
            //await _context.SaveChangesAsync();

            //// Trả về thông báo dựa trên trạng thái
            //return status switch
            //{
            //    "Đã Đăng ký" => "Bạn đã đăng ký thành công lịch hẹn và đủ điều kiện hiến máu.",
            //    "Không đủ điều kiện" => "Bạn đã đăng ký thành công lịch hẹn nhưng không đủ điều kiện hiến máu.",
            //    "Đang xét duyệt" => "Bạn đã đăng ký thành công lịch hẹn và đang chờ xét duyệt.",
            //    _ => "Bạn đã đăng ký thành công lịch hẹn."
            //};

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
