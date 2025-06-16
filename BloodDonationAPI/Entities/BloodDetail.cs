using System;
using System.Collections.Generic;

namespace BloodDonationAPI.Entities;

public partial class BloodDetail
{
    public int BloodDetailId { get; set; }

    public string? BloodType { get; set; }

    public int? Volume { get; set; }

    public int? AppointmentId { get; set; }

    public int? HospitalId { get; set; }

    public DateOnly? BloodDetailDate { get; set; }

    public string? BloodDetailStatus { get; set; }

    public string? Note { get; set; }

    public virtual AppointmentRecord? Appointment { get; set; }

    public virtual BloodBank? BloodTypeNavigation { get; set; }

    public virtual Hospital? Hospital { get; set; }
}
