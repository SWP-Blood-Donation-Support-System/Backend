using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BloodDonationAPI.Entities
{
    public class SurveyQuestion
    {
        [Key]
        public int QuestionId { get; set; }
        
        [Required]
        public string QuestionText { get; set; }
        
        [Required]
        public string QuestionType { get; set; } // SINGLE_CHOICE, TEXT
        
        public virtual ICollection<SurveyOption> Options { get; set; } = new List<SurveyOption>();
        
        public virtual ICollection<UserSurveyAnswer> UserAnswers { get; set; } = new List<UserSurveyAnswer>();
    }
} 