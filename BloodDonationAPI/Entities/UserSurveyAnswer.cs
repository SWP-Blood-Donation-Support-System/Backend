using System;
using System.Collections.Generic;

namespace BloodDonationAPI.Entities;

public partial class UserSurveyAnswer
{
    public int AnswerId { get; set; }

    public int? AppointmentId { get; set; }

    public int? QuestionId { get; set; }

    public int? OptionId { get; set; }

    public string? AdditionalText { get; set; }

    public DateTime? AnswerDate { get; set; }

    public virtual AppointmentRecord? Appointment { get; set; }

    public virtual SurveyOption? Option { get; set; }

    public virtual SurveyQuestion? Question { get; set; }
}
