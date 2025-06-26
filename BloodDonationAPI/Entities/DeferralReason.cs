using System;
using System.Collections.Generic;

namespace BloodDonationAPI.Entities;

public partial class DeferralReason
{
    public string ReasonCode { get; set; } = null!;

    public string ReasonText { get; set; } = null!;

    public int? MinDays { get; set; }

    public bool IsPermanent { get; set; }

    public string? Note { get; set; }

    public virtual ICollection<DonorDeferral> DonorDeferrals { get; set; } = new List<DonorDeferral>();
}
