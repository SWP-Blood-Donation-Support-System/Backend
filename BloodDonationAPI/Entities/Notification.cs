using System;
using System.Collections.Generic;

namespace BloodDonationAPI.Entities;

public partial class Notification
{
    public int EmergencyId { get; set; }

    public string? NotificationStatus { get; set; }

    public string? NotificationTitle { get; set; }

    public string? NotificationContent { get; set; }

    public int? BloodTypeId { get; set; }

    public virtual BloodBank? BloodType { get; set; }

    public virtual Emergency Emergency { get; set; } = null!;

    public virtual ICollection<NotificationRecipient> NotificationRecipients { get; set; } = new List<NotificationRecipient>();
}
