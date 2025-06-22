using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BloodDonationAPI.DTO
{
    public class SurveyQuestionDto
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public string QuestionType { get; set; }
        public List<SurveyOptionDto> Options { get; set; }
    }

    public class SurveyOptionDto
    {
        public int OptionId { get; set; }
        public string OptionText { get; set; }
    }





    public class SubmitAnswerDto
    {
        [Required]
        public int QuestionId { get; set; }
        public int? OptionId { get; set; }
        public string AnswerText { get; set; }
        [Required]
        public int EventId { get; set; }
    }

    public class UserAnswerDto
    {
        public int AnswerId { get; set; }
        public string Username { get; set; }
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public string QuestionType { get; set; }
        public int? OptionId { get; set; }
        public string OptionText { get; set; }
        public string AnswerText { get; set; }
        public DateTime CreatedAt { get; set; }
        public int EventId { get; set; }
    }


    public class SurveyAnswerDto
    {
      public int appointmentId { get; set; }
      public List<AnswerItemDto> Answers { get; set; } = new List<AnswerItemDto>();
    }

    public class AnswerItemDto
    {
        public int QuestionId { get; set; }
        public int OptionId { get; set; }
        public string? AdditionalText { get; set; }
    }

    public class  SurveyAnsweredByAppointmentIdDto
    {
        public int AppointmentId { get; set; }
        public string? Status { get; set; }
        public List<SurveyAnsweredItemsDto> AnsweredItems { get; set; } = new List<SurveyAnsweredItemsDto>();

    }
    public class SurveyAnsweredDto 
    { 
        public int? appointmentId { get; set; }
       public List<SurveyAnsweredItemsDto> AnsweredItems { get; set; } = new List<SurveyAnsweredItemsDto>();

    }
    public class SurveyAnsweredItemsDto
    {
        public int? QuestionId { get; set; }
        public string QuestionText { get; set; }
        public int? OptionId { get; set; }
        public string OptionText { get; set; }
        public string? AdditionalText { get; set; }

        public DateTime? AnswerDate { get; set; }
    }

    public class UpdataAppointmentStatusDto
    {
        public int AppointmentId { get; set; }
        public string Status { get; set; }
    }
} 