namespace BloodDonationAPI.DTO
{
    public class CheckInDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public int AppointmentId { get; set; }
        public bool CanDonate { get; set; }
    }
}
