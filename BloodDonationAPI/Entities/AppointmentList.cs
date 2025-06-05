using System;
using System.Collections.Generic;

namespace BloodDonationAPI.Entities;

public partial class AppointmentList
{
    public int AppointmentId { get; set; }

    public DateOnly? AppointmentDate { get; set; }

    public TimeOnly? AppointmentTime { get; set; }

    public string? AppointmentTitle { get; set; }

    public string? AppointmentContent { get; set; }

    public virtual ICollection<AppointmentHistory> AppointmentHistories { get; set; } = new List<AppointmentHistory>();
}
