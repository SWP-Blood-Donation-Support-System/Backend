using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BloodDonationAPI.Entities
{
    public class UserSurveyAnswer
    {
        [Key]
        public int AnswerId { get; set; }
        
        [Required]
        public string Username { get; set; }
        
        [Required]
        public int QuestionId { get; set; }
        
        public int? OptionId { get; set; }
        
        public string AnswerText { get; set; }
        
        public DateTime AnswerDate { get; set; } = DateTime.Now;
        
        [ForeignKey("Username")]
        public virtual User User { get; set; }
        
        [ForeignKey("QuestionId")]
        public virtual SurveyQuestion Question { get; set; }
        
        [ForeignKey("OptionId")]
        public virtual SurveyOption Option { get; set; }
    }
} 