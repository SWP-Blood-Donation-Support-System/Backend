using System;
using System.Collections.Generic;

namespace BloodDonationAPI.Entities;

public partial class Certificate
{
    public int CertificateId { get; set; }

    public int AppointmentId { get; set; }

    public string FullName { get; set; } = null!;

    public DateOnly DateOfBirth { get; set; }

    public string Address { get; set; } = null!;

    public string HospitalName { get; set; } = null!;

    public int BloodAmount { get; set; }

    public DateOnly DonationDate { get; set; }

    public string CertificateCode { get; set; } = null!;

    public DateOnly IssueDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual AppointmentRecord Appointment { get; set; } = null!;
}
