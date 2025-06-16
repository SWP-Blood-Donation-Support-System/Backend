using BloodDonationAPI.DTO;
using BloodDonationAPI.Entities;

namespace BloodDonationAPI.Service
{
    public interface ISurveyService
    {
        Task<List<SurveyQuestionDto>> GetAllQuestions();
        Task<string> SubmitAnswer(string username, SubmitAnswerDto dto);
        Task<List<UserAnswerDto>> GetUserAnswers(string username, int eventId);
    }
} 