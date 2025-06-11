namespace BloodDonationAPI.DTO
{
    public class AppointmentRegistrationDto
    {
        public int AppointmentHistoryId { get; set; }
        public string? Username { get; set; }
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public DateTime? AppointmentDate { get; set; }
        public string? AppointmentStatus { get; set; }
    }
}
