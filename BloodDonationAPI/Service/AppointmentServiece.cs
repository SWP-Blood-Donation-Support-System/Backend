using BloodDonationAPI.DTO;
using BloodDonationAPI.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
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
                    MaxParticipants = a.MaxParticipants,
                    BloodTypeRequired = a.BloodTypeRequired,
                    CurrentParticipants = a.CurrentParticipants
                })
                .ToListAsync();

        }
        public async Task<RegisterAppointmentResultDto> RegisterAppointment(string userName, RegisterAppointmentDto Dto)
        {
            //kiểm tra người dùng có tồn tại và đủ điều kiện đăng ký không
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == userName);
            if (user == null)
                return new RegisterAppointmentResultDto
                {
                    IsSuccess = false,
                    Message = "Người dùng không tồn tại.",
                    AppointmentId = null
                };
            //kiem tra người dùng có đủ điều kiện đăng ký không
            if (user.ProfileStatus != "Active")
            {
                return new RegisterAppointmentResultDto
                {
                    IsSuccess = false,
                    Message = "Tài khoản của bạn không đủ điều kiện đăng ký lịch hẹn.",
                    AppointmentId = null
                };
            }
            //kiểm tra người dùng có đang bị hoãn hiến máu không
            var today = DateOnly.FromDateTime(DateTime.Today);

            var activeDeferrals = await _context.DonorDeferrals
                .Where(d => d.Username == userName &&
                    (d.IsPermanent == true || (d.EndDate.HasValue && d.EndDate.Value >= today)))
                .Include(d => d.ReasonCodeNavigation)
                .ToListAsync();

            if (activeDeferrals.Any())
            {
                var reasons = string.Join("; ", activeDeferrals.Select(d =>
                    $"{d.ReasonCodeNavigation.ReasonText} - {(d.Note ?? "Không rõ lý do")}"
                ));

                return new RegisterAppointmentResultDto
                {
                    IsSuccess = false,
                    Message = $"Bạn hiện không thể đăng ký lịch hẹn.\nLý do: {reasons}",
                    AppointmentId = null
                };
            }

            //kiểm tra lịch hẹn có tồn tại không
            var appointment = await _context.Events
                .FirstOrDefaultAsync(a => a.EventId == Dto.eventId);

            if (appointment == null)
            {
                return new RegisterAppointmentResultDto
                {
                    IsSuccess = false,
                    Message = "Lịch hẹn không tồn tại.",
                    AppointmentId = null
                };
            }

            //kiểm tra xem đã đăng ký lịch hẹn này chưa
            bool alreadyRegistered = await _context.AppointmentRecords.AnyAsync(h =>
                h.Username == userName && h.EventId == Dto.eventId && h.Status != "Hủy");

            if (alreadyRegistered)
            {
                return new RegisterAppointmentResultDto
                {
                    IsSuccess = false,
                    Message = "Bạn đã đăng ký lịch hẹn này rồi.",
                    AppointmentId = null
                };
            }
            // nếu chưa có lịch hẹn thì thêm mới vào bảng AppointmentRecords
            var history = new AppointmentRecord
            {
                Username = userName,
                EventId = Dto.eventId,
                RegistrationDate = DateTime.Now,
                Status = "Đang xét duyệt"
            };

            

            _context.AppointmentRecords.Add(history);

            // cong them nguoi dang ky vao so luong nguoi dang ky cua su kien
            appointment.CurrentParticipants = (appointment.CurrentParticipants ?? 0) + 1;
            _context.Events.Update(appointment);
            await _context.SaveChangesAsync();
            return new RegisterAppointmentResultDto
            {
                IsSuccess = true,
                Message = "Bạn đã đăng ký thành công lịch hẹn.",
                AppointmentId = history.AppointmentId
            };

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

            var result = new List<AppointmentHistoryDto>();

            foreach (var h in records)
            {
                var deferral = await _context.DonorDeferrals
                    .Include(d => d.ReasonCodeNavigation)
                    .FirstOrDefaultAsync(d =>
                        d.Username == h.Username &&
                        d.StartDate == DateOnly.FromDateTime(h.RegistrationDate ?? DateTime.MinValue));

                result.Add(new AppointmentHistoryDto
                {
                    // thong tin lich hen
                    AppointmentId = h.AppointmentId,
                    EventId = h.EventId,
                    AppointmentDate = h.RegistrationDate,
                    AppointmentStatus = h.Status,

                    AppointmentDateOfAppointment = h.Event?.EventDate,
                    AppointmentTime = h.Event?.EventTime,
                    AppointmentTitle = h.Event?.EventTitle,
                    AppointmentContent = h.Event?.EventContent,
                    // thong tin hien mau
                    BloodType = h.BloodType,
                    DonationUnit = h.DonationUnit,
                    BloodStatus = h.BloodDetails.FirstOrDefault()?.BloodDetailStatus,
                    BloodLocation = h.BloodDetails.FirstOrDefault()?.Hospital?.HospitalName,

                    StaffNote = h.StaffNote, // Lưu lý do ngắn (ReasonText)
                    // thong tin li do tu choi neu co
                    DeferralReasonText = deferral?.ReasonCodeNavigation.ReasonText,
                    DeferralAdvice = deferral?.ReasonCodeNavigation.Note,
                    DeferralUserNote = deferral?.Note,

                    CanDonateAgainDate = deferral != null && !deferral.IsPermanent
        ? deferral.EndDate
        : null
                });
            }

            return result;

        }

        public async Task<bool> CancelAppointmentAsync(int appointmentId)
        {
            var appoinment = await _context.AppointmentRecords.FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

            if (appoinment == null || appoinment.Status == "Hủy")
            {
                return false; // Lịch hẹn không tồn tại
            }

            //kiem tra xem lich nay da duoc cong vao so nguoi dang ki chua (da xet duyet hay  khong)

            bool isDecreased = appoinment.Status == "Đã đủ điều kiện";
            appoinment.Status = "Hủy"; // Cập nhật trạng thái lịch hẹn
            _context.AppointmentRecords.Update(appoinment);

            if(isDecreased)
            {
                // Giảm số lượng người đăng ký của sự kiện
                var eventRecord = await _context.Events.FirstOrDefaultAsync(e => e.EventId == appoinment.EventId);
                if (eventRecord != null && (eventRecord.CurrentParticipants ?? 0) > 0)
                {
                    eventRecord.CurrentParticipants = (eventRecord.CurrentParticipants ?? 0) - 1;
                    _context.Events.Update(eventRecord);
                }

            }

            

            await _context.SaveChangesAsync(); // Lưu thay đổi vào cơ sở dữ liệu
            return true; // Trả về true nếu cập nhật thành công
        }

        public async Task<RegisterAppointmentResultDto> RegisterAppointmentV2(string userName, RegisterAppointmentDtoV2 Dto)
        {
            //kiểm tra người dùng có tồn tại và đủ điều kiện đăng ký không
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == userName);
            if (user == null)
                return new RegisterAppointmentResultDto
                {
                    IsSuccess = false,
                    Message = "Người dùng không tồn tại.",
                    AppointmentId = null
                };
            // kiem tra người dùng có đủ điều kiện đăng ký không
            if (user.ProfileStatus != "Active")
            {
                return new RegisterAppointmentResultDto
                {
                    IsSuccess = false,
                    Message = "Tài khoản của bạn không đủ điều kiện đăng ký lịch hẹn.",
                    AppointmentId = null
                };
            }
            //kiểm tra người dùng có đang bị hoãn hiến máu không
            var today = DateOnly.FromDateTime(DateTime.Today);

            var activeDeferrals = await _context.DonorDeferrals
                .Where(d => d.Username == userName &&
                    (d.IsPermanent == true || (d.EndDate.HasValue && d.EndDate.Value >= today)))
                .Include(d => d.ReasonCodeNavigation)
                .ToListAsync();

            if (activeDeferrals.Any())
            {
                var reasons = string.Join("; ", activeDeferrals.Select(d =>
                    $"{d.ReasonCodeNavigation.ReasonText} - {(d.Note ?? "Không rõ lý do")}"
                ));

                return new RegisterAppointmentResultDto
                {
                    IsSuccess = false,
                    Message = $"Bạn hiện đang không thể đăng ký lịch hẹn.\nLý do: {reasons}",
                    AppointmentId = null
                };
            }

            //kiểm tra lịch hẹn có tồn tại không
            var appointment = await _context.Events
                .FirstOrDefaultAsync(a => a.EventId == Dto.eventId);

            if (appointment == null)
            {
                return new RegisterAppointmentResultDto
                {
                    IsSuccess = false,
                    Message = "Lịch hẹn không tồn tại.",
                    AppointmentId = null
                };
            }

            //kiểm tra xem đã đăng ký lịch hẹn này chưa
            bool alreadyRegistered = await _context.AppointmentRecords.AnyAsync(h =>
                h.Username == userName && h.EventId == Dto.eventId && h.Status != "Hủy");

            if (alreadyRegistered)
            {
                return new RegisterAppointmentResultDto
                {
                    IsSuccess = false,
                    Message = "Bạn đã đăng ký lịch hẹn này rồi.",
                    AppointmentId = null
                };
            }
            //// nếu chưa có lịch hẹn thì thêm mới vào bảng AppointmentRecords
            //var history = new AppointmentRecord
            //{
            //    Username = userName,
            //    EventId = Dto.eventId,
            //    RegistrationDate = DateTime.Now,
            //    Status = "Đang xét duyệt"
            //};

            //_context.AppointmentRecords.Add(history);
            //await _context.SaveChangesAsync();
            //return new RegisterAppointmentResultDto
            //{
            //    IsSuccess = true,
            //    Message = "Bạn đã đăng ký thành công lịch hẹn.",
            //    AppointmentId = history.AppointmentId
            //};

            // lay tong cau hoi trong bang SurveyQuestions
            var totalQuestion = await _context.SurveyQuestions.CountAsync();
            // lay tat cac cac cau tra loi cua nguoi dung moi (bo cac cai trung lap di boi vi co cau hoi chon multiple choice)
            var answeredQuestions = Dto.userSurveyAnswerDtos.Select(a => a.QuestionId)
                .Distinct()
                .Count();
            // kiem tra xem nguoi dung da tra loi het cac cau hoi chua
            if (answeredQuestions < totalQuestion)
            {
                return new RegisterAppointmentResultDto
                {
                    IsSuccess = false,
                    Message = $"Bạn cần trả lời tất cả {totalQuestion} câu hỏi khảo sát để đăng ký lịch hẹn.",
                    AppointmentId = null
                };
            }
            // them moi vao bang khi da tra loi day du cac cau hoi
            var newAppointment = new AppointmentRecord
            {
                Username = userName,
                EventId = Dto.eventId,
                RegistrationDate = DateTime.Now,
                Status = "Đang xét duyệt"
            };
            _context.AppointmentRecords.Add(newAppointment);
            await _context.SaveChangesAsync();

            // luu cac cau tra loi cua nguoi dung vao bang UserSurveyAnswers

            foreach (var answer in Dto.userSurveyAnswerDtos)
            {
                var userSurveyAnswer = new UserSurveyAnswer
                {
                    AppointmentId = newAppointment.AppointmentId,
                    QuestionId = answer.QuestionId,
                    OptionId = answer.OptionId,
                    AdditionalText = answer.AdditionalText,
                    AnswerDate = DateTime.Now
                };
                _context.UserSurveyAnswers.Add(userSurveyAnswer);
            }
            await _context.SaveChangesAsync();

            // dung de kiem tra cac cau hoi va cap nhat trang thai cho appointmentRecord 
            var isEligible = await CheckUserSurveyAndSetStatus(newAppointment.AppointmentId);
            //dat trang thai tui vao cau tra loi
            var status = isEligible switch
            {
                true => "Đã đủ điều kiện",
                false => "Không đủ điều kiện",
                null => "Đang xét duyệt"
            };

            newAppointment.Status = status;
            _context.AppointmentRecords.Update(newAppointment);

            if (isEligible == true) 
            {
                appointment.CurrentParticipants = (appointment.CurrentParticipants ?? 0) + 1;
                _context.Events.Update(appointment);
            }
            await _context.SaveChangesAsync();

            var message = isEligible switch
            {
                true => "Bạn đã đăng ký thành công lịch hẹn và đủ điều kiện hiến máu.",
                false => "Bạn đã đăng ký thành công lịch hẹn nhưng không đủ điều kiện hiến máu.",
                null => "Bạn đã đăng ký thành công lịch hẹn và đang chờ xét duyệt."
            };
            return new RegisterAppointmentResultDto
            {
                IsSuccess = true,
                Message = message,
                AppointmentId = newAppointment.AppointmentId
            };
        }

        public async Task<bool?> CheckUserSurveyAndSetStatus(int appointmentID)
        {
            var answers = await _context.UserSurveyAnswers
                .Where(a => a.AppointmentId == appointmentID)
                .Select(a => a.Option.IsEligible).ToListAsync();
            if (answers.Any(a => a == false))
            {
                return false; // Nếu có bất kỳ câu trả lời nào không đủ điều kiện, trả về false
            }
            else if (answers.All(a => a == true))
            {
                return true; // Nếu tất cả câu trả lời đều đủ điều kiện, trả về true
            }
            return null; // Nếu không có câu trả lời nào, trả về null

        }
    }
}
