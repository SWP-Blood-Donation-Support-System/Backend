using System;
using System.Collections.Generic;

namespace BloodDonationAPI.Entities;

public partial class SurveyQuestion
{
    public int QuestionId { get; set; }

    public string? QuestionText { get; set; }

    public string? QuestionType { get; set; }

    public virtual ICollection<SurveyOption> SurveyOptions { get; set; } = new List<SurveyOption>();

    public virtual ICollection<UserSurveyAnswer> UserSurveyAnswers { get; set; } = new List<UserSurveyAnswer>();
}
