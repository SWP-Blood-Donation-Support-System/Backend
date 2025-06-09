using System;
using System.Collections.Generic;

namespace BloodDonationAPI.Entities;

public partial class Hospital
{
    public int HospitalId { get; set; }

    public string? HospitalName { get; set; }

    public string? HospitalAddress { get; set; }

    public string? HospitalImage { get; set; }

    public string? HospitalPhone { get; set; }

    public virtual ICollection<BloodMove> BloodMoves { get; set; } = new List<BloodMove>();

    public virtual ICollection<Emergency> Emergencies { get; set; } = new List<Emergency>();
}
