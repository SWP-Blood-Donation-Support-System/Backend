using System;
using System.Collections.Generic;

namespace BloodDonationAPI.Entities;

public partial class DonationHistory
{
    public int DonationHistoryId { get; set; }

    public string? Username { get; set; }

    public int? BloodTypeId { get; set; }

    public DateOnly? DonationDate { get; set; }

    public string? DonationStatus { get; set; }

    public int? DonationUnit { get; set; }

    public virtual BloodBank? BloodType { get; set; }

    public virtual Certificate? Certificate { get; set; }

    public virtual User? UsernameNavigation { get; set; }
}
