using System;
using System.Collections.Generic;

namespace BloodDonationAPI.Entities;

public partial class BloodBank
{
    public int BloodTypeId { get; set; }

    public string? BloodTypeName { get; set; }

    public int? Unit { get; set; }

    public int? DonationHistoryId { get; set; }

    public DateOnly? ExpiryDate { get; set; }

    public string? Status { get; set; }

    public virtual DonationHistory? DonationHistory { get; set; }

    public virtual ICollection<BloodMove> BloodMoves { get; set; } = new List<BloodMove>();
}