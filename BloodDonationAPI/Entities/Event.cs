using System;
using System.Collections.Generic;

namespace BloodDonationAPI.Entities;

public partial class Event
{
    public int EventId { get; set; }

    public DateOnly? EventDate { get; set; }

    public TimeOnly? EventTime { get; set; }

    public string? EventTitle { get; set; }

    public string? EventContent { get; set; }

    public string? Location { get; set; }

    public int? MaxParticipants { get; set; }

    public virtual ICollection<AppointmentRecord> AppointmentRecords { get; set; } = new List<AppointmentRecord>();
}
