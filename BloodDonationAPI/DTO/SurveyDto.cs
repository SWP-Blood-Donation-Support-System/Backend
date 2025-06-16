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
    }
} 