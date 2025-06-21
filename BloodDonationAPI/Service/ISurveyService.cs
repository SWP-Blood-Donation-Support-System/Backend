using BloodDonationAPI.DTO;
using BloodDonationAPI.Entities;

namespace BloodDonationAPI.Service
{
    public interface ISurveyService
    {
        Task<List<SurveyQuestionDto>> GetAllQuestions();
        //Task<string> SubmitAnswer(string username, SubmitAnswerDto dto);
        //Task<List<UserAnswerDto>> GetUserAnswers(string username, int eventId);
        Task<string> SubmitSurveyAnswersAsync(SurveyAnswerDto dto);

        Task<SurveyAnsweredDto> GetAnsweredByAppointmentIdAsync(int appointmentId);
        Task<List<SurveyAnsweredDto>> GetAllAnsweredOfAppointmentHavePendinStatusAsync();
    }
} 