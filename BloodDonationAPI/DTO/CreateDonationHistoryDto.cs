namespace BloodDonationAPI.DTO
{
    public class CreateDonationHistoryDto
    {
        public string Username { get; set; } = null!;
        public string BloodType { get; set; } = null!;
        public DateOnly DonationDate { get; set; }
        public string DonationStatus { get; set; } = "Donated";
        public int DonationUnit { get; set; } 
    }
}
