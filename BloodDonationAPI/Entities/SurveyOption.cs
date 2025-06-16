using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BloodDonationAPI.Entities
{
    public class SurveyOption
    {
        [Key]
        public int OptionId { get; set; }
        
        [Required]
        public string OptionText { get; set; }
        
        [Required]
        public int QuestionId { get; set; }
        
        [ForeignKey("QuestionId")]
        public virtual SurveyQuestion Question { get; set; }
        
        public virtual ICollection<UserSurveyAnswer> UserAnswers { get; set; } = new List<UserSurveyAnswer>();
    }
} 