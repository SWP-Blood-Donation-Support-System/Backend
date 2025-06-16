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
                .Include(q => q.Options)
                .ToListAsync();

            return questions.Select(q => new SurveyQuestionDto
            {
                QuestionId = q.QuestionId,
                QuestionText = q.QuestionText,
                QuestionType = q.QuestionType,
                Options = q.Options.Select(o => new SurveyOptionDto
                {
                    OptionId = o.OptionId,
                    OptionText = o.OptionText
                }).ToList()
            }).ToList();
        }

        public async Task<string> SubmitAnswer(string username, SubmitAnswerDto dto)
        {
            var question = await _context.SurveyQuestions
                .FirstOrDefaultAsync(q => q.QuestionId == dto.QuestionId);

            if (question == null)
                return "Question not found";

            var eventExists = await _context.Events.AnyAsync(e => e.EventId == dto.EventId);
            if (!eventExists)
                return "Event not found";

            if (question.QuestionType == "SINGLE_CHOICE" && !dto.OptionId.HasValue)
                return "OptionId is required for single choice questions";

            if (question.QuestionType == "TEXT" && string.IsNullOrEmpty(dto.AnswerText))
                return "AnswerText is required for text questions";

            var answer = new UserSurveyAnswer
            {
                Username = username,
                QuestionId = dto.QuestionId,
                OptionId = dto.OptionId,
                AnswerText = dto.AnswerText,
                AnswerDate = DateTime.Now,
                EventId = dto.EventId
            };

            _context.UserSurveyAnswers.Add(answer);
            await _context.SaveChangesAsync();

            return "Answer submitted successfully";
        }

        public async Task<List<UserAnswerDto>> GetUserAnswers(string username, int eventId)
        {
            var answers = await _context.UserSurveyAnswers
                .Where(a => a.Username == username && a.EventId == eventId)
                .Include(a => a.Question)
                .Include(a => a.Option)
                .Include(a => a.Event)
                .OrderBy(a => a.AnswerDate)
                .ToListAsync();

            return answers.Select(a => new UserAnswerDto
            {
                AnswerId = a.AnswerId,
                Username = a.Username,
                QuestionId = a.QuestionId,
                QuestionText = a.Question.QuestionText,
                QuestionType = a.Question.QuestionType,
                OptionId = a.OptionId,
                OptionText = a.Option?.OptionText,
                AnswerText = a.AnswerText,
                CreatedAt = a.AnswerDate,
                EventId = a.EventId
            }).ToList();
        }
    }
} 