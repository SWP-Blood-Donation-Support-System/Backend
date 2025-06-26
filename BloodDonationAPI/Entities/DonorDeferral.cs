using System;
using System.Collections.Generic;

namespace BloodDonationAPI.Entities;

public partial class DonorDeferral
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string ReasonCode { get; set; } = null!;

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public bool IsPermanent { get; set; }

    public string? Note { get; set; }

    public virtual DeferralReason ReasonCodeNavigation { get; set; } = null!;

    public virtual User UsernameNavigation { get; set; } = null!;
}
