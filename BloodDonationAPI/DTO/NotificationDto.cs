namespace BloodDonationAPI.DTO
{
    public class NotificationDto
    {
        public int NotificationId { get; set; }
        public int EmergencyId { get; set; }
        public string NotificationStatus { get; set; }
        public string NotificationTitle { get; set; }
        public string NotificationContent { get; set; }
        public DateOnly NotificationDate { get; set; }
        public string BloodType { get; set; }
        public int RequiredUnits { get; set; }
        public string HospitalName { get; set; }
        public string? ResponseStatus { get; set; }
        public DateTime? ResponseDate { get; set; }
        public List<NotificationRecipientDto> Recipients { get; set; } = new List<NotificationRecipientDto>();
    }

    public class NotificationRecipientDto
    {
        public string Username { get; set; }
        public string FullName { get; set; }
        public string ResponseStatus { get; set; }
        public DateTime? ResponseDate { get; set; }
        public DateOnly? ResponseGo { get; set; }
        public TimeOnly? ResponseTime { get; set; }
    }

    public class NotificationRecipientResponseDto
    {
        public string ResponseStatus { get; set; }
        public DateOnly? ResponseGo { get; set; }
        public TimeOnly? ResponseTime { get; set; }
    }
} 