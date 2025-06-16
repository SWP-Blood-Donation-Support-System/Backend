using System;
using System.Collections.Generic;

namespace BloodDonationAPI.Entities;

public partial class BloodBank
{
    public string BloodType { get; set; } = null!;

    public int? BloodVolumeTotal { get; set; }

    public string? BloodBankStatus { get; set; }

    public virtual ICollection<BloodDetail> BloodDetails { get; set; } = new List<BloodDetail>();
}
