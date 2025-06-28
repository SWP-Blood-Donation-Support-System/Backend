namespace BloodDonationAPI.DTO
{
    public class RegisterAppointmentDto
    {
        public int eventId { get; set; }
    }

    public class RegisterAppointmentDtoV2
    {
        public int eventId { get; set; }
       public List<UserSurveyAnswerDto> userSurveyAnswerDtos { get; set; } = new List<UserSurveyAnswerDto>();
    }
    public class UserSurveyAnswerDto{
        public int QuestionId { get; set; }
        public int? OptionId { get; set; }
        public string? AdditionalText { get; set; }
    }
    public class RegisterAppointmentResultDto
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = "";
        public int? AppointmentId { get; set; }

    }
}
