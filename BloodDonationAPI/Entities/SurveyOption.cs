using System;
using System.Collections.Generic;

namespace BloodDonationAPI.Entities;

public partial class SurveyOption
{
    public int OptionId { get; set; }

    public int? QuestionId { get; set; }

    public string? OptionText { get; set; }

    public bool? IsEligible { get; set; }

    public int? DisplayOrder { get; set; }

    public bool? RequireText { get; set; }

    public virtual SurveyQuestion? Question { get; set; }

    public virtual ICollection<UserSurveyAnswer> UserSurveyAnswers { get; set; } = new List<UserSurveyAnswer>();
}
