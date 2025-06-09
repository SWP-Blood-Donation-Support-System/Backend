using System;
using System.Collections.Generic;

namespace BloodDonationAPI.Entities;

public partial class BloodMove
{
    public int BloodMoveId { get; set; }

    public int? BloodTypeId { get; set; }

    public int? Unit { get; set; }

    public int? HospitalId { get; set; }

    public DateOnly? DateMove { get; set; }

    public string? Note { get; set; }

    public virtual BloodBank? BloodType { get; set; }

    public virtual Hospital? Hospital { get; set; }
}
