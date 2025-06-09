using System;
using System.Collections.Generic;

namespace BloodDonationAPI.Entities;

public partial class AppointmentHistory
{
    public int AppointmentHistoryId { get; set; }

    public string? Username { get; set; }

    public int? AppointmentId { get; set; }

    public DateTime? AppointmentDate { get; set; }

    public string? AppointmentStatus { get; set; }

    public virtual AppointmentList? Appointment { get; set; }

    public virtual User? UsernameNavigation { get; set; }
}
