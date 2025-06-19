using System;
using System.Collections.Generic;

namespace BloodDonationAPI.Entities;

public partial class Notification
{
    public int NotificationId { get; set; }

    public int? EmergencyId { get; set; }

    public string? NotificationStatus { get; set; }

    public string? NotificationTitle { get; set; }

    public string? NotificationContent { get; set; }

    public DateOnly? NotificationDate { get; set; }

    public virtual Emergency? Emergency { get; set; }

    public virtual ICollection<NotificationRecipient> NotificationRecipients { get; set; } = new List<NotificationRecipient>();
}
