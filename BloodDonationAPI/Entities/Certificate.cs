using System;
using System.Collections.Generic;

namespace BloodDonationAPI.Entities;

public partial class Certificate
{
    public int DonationHistoryId { get; set; }

    public string? CertificateCode { get; set; }

    public DateOnly? IssueDate { get; set; }

    public virtual DonationHistory DonationHistory { get; set; } = null!;
}
