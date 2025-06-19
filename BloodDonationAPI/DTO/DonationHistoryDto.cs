namespace BloodDonationAPI.DTO
{
    public class DonationHistoryDto
    {
        public int DonationHistoryId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string BloodType { get; set; } = string.Empty;
        public DateOnly? DonationDate { get; set; }
        public string DonationStatus { get; set; } = string.Empty;
        public int? DonationUnit { get; set; }
    }
}
