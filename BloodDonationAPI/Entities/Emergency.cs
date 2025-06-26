using System;
using System.Collections.Generic;

namespace BloodDonationAPI.Entities;

public partial class Emergency
{
    public int EmergencyId { get; set; }

    public string? Username { get; set; }

    public DateOnly? EmergencyDate { get; set; }

    public string? BloodType { get; set; }

    public string? EmergencyStatus { get; set; }

    public string? EmergencyNote { get; set; }

    public int? RequiredUnits { get; set; }

    public int? HospitalId { get; set; }

    public virtual Hospital? Hospital { get; set; }

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual User? UsernameNavigation { get; set; }
}
