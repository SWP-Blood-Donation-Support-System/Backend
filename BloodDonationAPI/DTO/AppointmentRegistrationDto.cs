namespace BloodDonationAPI.DTO
{
    public class AppointmentRegistrationDto
    {
        public int AppointmentId { get; set; }
        public string? Username { get; set; }
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? AppointmentStatus { get; set; }

        public string? BloodType { get; set; }


    }
}
