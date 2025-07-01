namespace BloodDonationAPI.DTO
{
    public class DonateDto
    {
        public int AppointmentId { get; set; }
        public string? BloodType { get; set; }
        public int Volume { get; set; } // Volume in milliliters
       
    }
}
