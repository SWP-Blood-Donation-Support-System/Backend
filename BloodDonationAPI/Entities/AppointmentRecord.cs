using System;
using System.Collections.Generic;

namespace BloodDonationAPI.Entities;

public partial class AppointmentRecord
{
    public int AppointmentId { get; set; }

    public string? Username { get; set; }

    public int? EventId { get; set; }

    public DateTime? RegistrationDate { get; set; }

    public string? Status { get; set; }

    public string? BloodType { get; set; }

    public int? DonationUnit { get; set; }

    public string? StaffNote { get; set; }

    public virtual ICollection<BloodDetail> BloodDetails { get; set; } = new List<BloodDetail>();

    public virtual ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();

    public virtual Event? Event { get; set; }

    public virtual ICollection<UserSurveyAnswer> UserSurveyAnswers { get; set; } = new List<UserSurveyAnswer>();

    public virtual User? UsernameNavigation { get; set; }
}
