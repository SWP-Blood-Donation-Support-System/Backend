using System;
using System.Collections.Generic;

namespace BloodDonationAPI.Entities;

public partial class Certificate
{
    public int AppointmentId { get; set; }

    public string? CertificateCode { get; set; }

    public DateOnly? IssueDate { get; set; }

    public virtual AppointmentRecord Appointment { get; set; } = null!;
}
