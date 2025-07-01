using BloodDonationAPI.DTO;
using BloodDonationAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace BloodDonationAPI.Service
{
    public class SurveyService : ISurveyService
    {
        private readonly BloodDonationSystemContext _context;

        public SurveyService(BloodDonationSystemContext context)
        {
            _context = context;
        }

        public async Task<List<SurveyQuestionDto>> GetAllQuestions()
        {
            var questions = await _context.SurveyQuestions
                .Include(q => q.SurveyOptions)
                .ToListAsync();

            return questions.Select(q => new SurveyQuestionDto
            {
                QuestionId = q.QuestionId,
                QuestionText = q.QuestionText,
                QuestionType = q.QuestionType,
                Options = q.SurveyOptions.Select(o => new SurveyOptionDto
                {
                    OptionId = o.OptionId,
                    OptionText = o.OptionText,
                    displayOrder = o.DisplayOrder, // Thêm thuộc tính displayOrder
                    requireText = o.RequireText // Thêm thuộc tính requireText
                }).ToList()
            }).ToList();
        }

        public  async  Task<SurveyAnsweredByAppointmentIdDto?> GetAnsweredByAppointmentIdAsync(int appointmentId)
        {
           var answered =  await _context.UserSurveyAnswers
                .Where(a => a.AppointmentId == appointmentId)
                .Include(a => a.Question)
                .Include(a => a.Option)
                .Select(a => new SurveyAnsweredItemsDto
                {
                    
                    QuestionId = a.QuestionId,
                    QuestionText = a.Question.QuestionText,
                    OptionId = a.OptionId,
                    OptionText = a.Option.OptionText,
                    AdditionalText = a.AdditionalText,
                    AnswerDate = a.AnswerDate
                }).ToListAsync();

            if (answered == null || !answered.Any())
            {
                return null; // Không tìm thấy câu trả lời cho cuộc hẹn này
            }
                var result = new SurveyAnsweredByAppointmentIdDto
                {
                    AppointmentId = appointmentId,
                    Status = _context.AppointmentRecords
                        .Where(a => a.AppointmentId == appointmentId)
                        .Select(a => a.Status)
                        .FirstOrDefault(), // Lấy trạng thái của cuộc hẹn
                    AnsweredItems = answered
                };
            return result;

        }

        public async Task<string> SubmitSurveyAnswersAsync(SurveyAnswerDto dto)
        {
           if(dto.appointmentId <= 0 || dto.Answers == null || !dto.Answers.Any())
           {
              throw new ArgumentException("appointmentID hoặc câu trả lời không hợp lệ.");
            }
            // dùng để xóa dữ liệu cũ nếu có khi làm lại khảo sát
            var existingAnswers =  _context.UserSurveyAnswers
                .Where(a => a.AppointmentId == dto.appointmentId);  
            _context.UserSurveyAnswers.RemoveRange(existingAnswers);
            // them cau tra lời mới vao db
            var answers = dto.Answers.Select(a => new UserSurveyAnswer
            {
                AppointmentId = dto.appointmentId,
                QuestionId = a.QuestionId,
                OptionId = a.OptionId,
                AdditionalText = a.AdditionalText,
                AnswerDate = DateTime.Now
            }).ToList();
            await _context.UserSurveyAnswers.AddRangeAsync(answers);
            await _context.SaveChangesAsync();

            //kiem tra ket qua tra loi
            var checkResult = await CheckUserAnsweredSurvey(dto.appointmentId);

            // kiem tra xem lịch hẹn có tồn tại không
            var appointment = await _context.AppointmentRecords
                .FirstOrDefaultAsync(a => a.AppointmentId == dto.appointmentId);
            if (appointment == null)
            {
                throw new ArgumentException("Lịch hẹn không tồn tại.");
            }

            // kiem tra va gan cac status tuy theo ket qua tra loi
            var status = checkResult switch
            {
                true => "Đã đủ điều kiện",
                false => "Không đủ điều kiện",
                null => "Đang xét duyệt"
            };

            //luu status vao appointmentRecord
            appointment.Status = status;
            _context.AppointmentRecords.Update(appointment);
            await _context.SaveChangesAsync();

            var message = checkResult switch
            {
                true => "Câu trả lời đã được lưu thành công. Bạn đủ điều kiện hiến máu.",
                false => "Câu trả lời đã được lưu thành công. Bạn không đủ điều kiện hiến máu.",
                null => "Câu trả lời đã được lưu thành công. Đang chờ xét duyệt."
            };

            return message;
        }

        public async Task<bool?> CheckUserAnsweredSurvey(int appointmentID)
        {
            var answers = await _context.UserSurveyAnswers
                .Where(a => a.AppointmentId == appointmentID)
                .Select(a => a.Option.IsEligible).ToListAsync();

            if (answers.Any(a => a == false))
            {
                return false; // Nếu có bất kỳ câu trả lời nào không đủ điều kiện, trả về false
            }
            if (answers.All(a => a == true))
            {
                return true; // Nếu không có câu trả lời nào, coi như không đủ điều kiện
            }

            return null;
        }

        public async Task<List<SurveyAnsweredDto>> GetAllAnsweredOfAppointmentHavePendinStatusAsync()
        {
            var answers =  await _context.UserSurveyAnswers
                .Include(a => a.Question)
                .Include(a => a.Option)
                .Where(a => a.Appointment.Status == "Đang xét duyệt") // Chỉ lấy những câu trả lời có lịch hẹn đã có trạng thái ddang xet duyệt de xem xét
                .GroupBy(a => a.AppointmentId) // Nhóm theo AppointmentId để gom các câu trả lời của cùng một cuộc hẹn
                .Select(g => new SurveyAnsweredDto
                {
                    appointmentId = g.Key,
                    AnsweredItems = g.Select(a => new SurveyAnsweredItemsDto
                    {
                        QuestionId = a.QuestionId,
                        QuestionText = a.Question.QuestionText,
                        OptionId = a.OptionId,
                        OptionText = a.Option.OptionText,
                        AdditionalText = a.AdditionalText,
                        AnswerDate = a.AnswerDate
                    }).ToList()
                }).ToListAsync();


            return answers;
        }

        public async Task<bool> UpdateAppointmentStatus(UpdataAppointmentStatusDto dto)
        {
            var appointment = await _context.AppointmentRecords
                .FirstOrDefaultAsync(a => a.AppointmentId == dto.AppointmentId);

            if (appointment == null)
            {
                return false; // Lịch hẹn không tồn tại
            }

            bool shouldIncrease = dto.Status == "Đã đủ điều kiện" && appointment.Status != "Đã đủ điều kiện";

            appointment.Status = dto.Status;
            _context.AppointmentRecords.Update(appointment);
            if (shouldIncrease) 
            {
                var eventRecord = await _context.Events
                    .FirstOrDefaultAsync(e => e.EventId == appointment.EventId);
                if (eventRecord != null)
                {
                    eventRecord.CurrentParticipants = (eventRecord.CurrentParticipants ?? 0) + 1;
                    _context.Events.Update(eventRecord);
                }
            }
            await _context.SaveChangesAsync();
            return true; // Cập nhật thành công
        }


        //public async Task<string> SubmitAnswer(string username, SubmitAnswerDto dto)
        //{
        //    var question = await _context.SurveyQuestions
        //        .FirstOrDefaultAsync(q => q.QuestionId == dto.QuestionId);

        //    if (question == null)
        //        return "Question not found";

        //    var eventExists = await _context.Events.AnyAsync(e => e.EventId == dto.EventId);
        //    if (!eventExists)
        //        return "Event not found";

        //    if (question.QuestionType == "SINGLE_CHOICE" && !dto.OptionId.HasValue)
        //        return "OptionId is required for single choice questions";

        //    if (question.QuestionType == "TEXT" && string.IsNullOrEmpty(dto.AnswerText))
        //        return "AnswerText is required for text questions";

        //    var answer = new UserSurveyAnswer
        //    {
        //        Username = username,
        //        QuestionId = dto.QuestionId,
        //        OptionId = dto.OptionId,
        //        AnswerText = dto.AnswerText,
        //        AnswerDate = DateTime.Now,
        //        EventId = dto.EventId
        //    };

        //    _context.UserSurveyAnswers.Add(answer);
        //    await _context.SaveChangesAsync();

        //    return "Answer submitted successfully";
        //}

        //public async Task<List<UserAnswerDto>> GetUserAnswers(string username, int eventId)
        //{
        //    var answers = await _context.UserSurveyAnswers
        //        .Where(a => a.Username == username && a.EventId == eventId)
        //        .Include(a => a.Question)
        //        .Include(a => a.Option)
        //        .Include(a => a.Event)
        //        .OrderBy(a => a.AnswerDate)
        //        .ToListAsync();

        //    return answers.Select(a => new UserAnswerDto
        //    {
        //        AnswerId = a.AnswerId,
        //        Username = a.Username,
        //        QuestionId = a.QuestionId,
        //        QuestionText = a.Question.QuestionText,
        //        QuestionType = a.Question.QuestionType,
        //        OptionId = a.OptionId,
        //        OptionText = a.Option?.OptionText,
        //        AnswerText = a.AnswerText,
        //        CreatedAt = a.AnswerDate,
        //        EventId = a.EventId
        //    }).ToList();
        //}
    }
} 