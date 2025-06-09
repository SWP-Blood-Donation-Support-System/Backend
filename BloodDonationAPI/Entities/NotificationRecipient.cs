using System;
using System.Collections.Generic;

namespace BloodDonationAPI.Entities;

public partial class NotificationRecipient
{
    public int NotificationRecipientId { get; set; }

    public int? NotifivationId { get; set; }

    public string? Username { get; set; }

    public string? ResponseStatus { get; set; }

    public DateTime? ResponseDate { get; set; }

    public virtual Notification? Notifivation { get; set; }

    public virtual User? UsernameNavigation { get; set; }
}
