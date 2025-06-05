using System;
using System.Collections.Generic;

namespace BloodDonationAPI.Entities;

public partial class Report
{
    public int ReportId { get; set; }

    public string? Username { get; set; }

    public DateOnly? ReportDate { get; set; }

    public string? ReportType { get; set; }

    public string? ReportContent { get; set; }

    public virtual User? UsernameNavigation { get; set; }
}
