namespace BloodDonationAPI.DTO
{
    public class DonateDto
    {
        public int AppointmentId { get; set; }
        public string? BloodType { get; set; }
        public int Volume { get; set; } // Volume in milliliters
        public bool CanDonate { get; set; } = true; // Mặc định là có thể hiến máu
        public string? StaffNote { get; set; } // Ghi chú khi người hiến có vấn đề sức khỏe
    }
}
