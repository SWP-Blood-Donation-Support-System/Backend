using System;
using System.Collections.Generic;

namespace BloodDonationAPI.Entities;

public partial class BloodBank
{
    public int BloodTypeId { get; set; }

    public string? BloodTypeName { get; set; }

    public int? Unit { get; set; }

    public virtual ICollection<BloodMove> BloodMoves { get; set; } = new List<BloodMove>();

    public virtual ICollection<DonationHistory> DonationHistories { get; set; } = new List<DonationHistory>();

    public virtual ICollection<Emergency> Emergencies { get; set; } = new List<Emergency>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
