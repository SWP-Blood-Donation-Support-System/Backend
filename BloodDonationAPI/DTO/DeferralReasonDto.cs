namespace BloodDonationAPI.DTO
{
    public class DeferralReasonDto
    {
        public string ReasonCode { get; set; } = null!;
        public string ReasonText { get; set; } = null!;
        public int? MinDays { get; set; }
        public bool IsPermanent { get; set; }
        public string? Note { get; set; }
        public int? MinHours { get; set; }
        public int? MinMinutes { get; set; }
    }
}
