namespace BloodDonationAPI.DTO
{
    public class EventDto
    {
        public int EventId { get; set; }

        public DateOnly? EventDate { get; set; }

        public TimeOnly? EventTime { get; set; }

        public string? EventTitle { get; set; }

        public string? EventContent { get; set; }

        public string? Location { get; set; }

        public int? MaxParticipants { get; set; }

        public int? CurrentParticipants { get; set; }
        public string? BloodTypeRequired { get; set; }
    }
}
