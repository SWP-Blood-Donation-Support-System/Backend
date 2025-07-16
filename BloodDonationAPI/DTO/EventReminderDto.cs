namespace BloodDonationAPI.DTO
{
    public class EventReminderDto
    {
        public string UserEmail { get; set; } = null!;
        public string UserFullName { get; set; } = null!;
        public string EventTitle { get; set; } = null!;
        public string EventContent { get; set; } = null!;
        public DateOnly EventDate { get; set; }
        public TimeOnly EventTime { get; set; }
        public string Location { get; set; } = null!;
        public string BloodTypeRequired { get; set; } = null!;
    }
}
