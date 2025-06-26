using System;
using System.Collections.Generic;

namespace BloodDonationAPI.Entities;

public partial class User
{
    public string Username { get; set; } = null!;

    public string? Password { get; set; }

    public string? Email { get; set; }

    public string? Role { get; set; }

    public string? FullName { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? Gender { get; set; }

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public string? ProfileStatus { get; set; }

    public string? BloodType { get; set; }

    public virtual ICollection<AppointmentRecord> AppointmentRecords { get; set; } = new List<AppointmentRecord>();

    public virtual ICollection<Blog> Blogs { get; set; } = new List<Blog>();

    public virtual ICollection<DonorDeferral> DonorDeferrals { get; set; } = new List<DonorDeferral>();

    public virtual ICollection<Emergency> Emergencies { get; set; } = new List<Emergency>();

    public virtual ICollection<NotificationRecipient> NotificationRecipients { get; set; } = new List<NotificationRecipient>();

    public virtual ICollection<Report> Reports { get; set; } = new List<Report>();
}
