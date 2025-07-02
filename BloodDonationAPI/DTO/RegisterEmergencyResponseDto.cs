namespace BloodDonationAPI.DTO
{
    public class RegisterEmergencyResponseDto
    {
        public string Message { get; set; }
        public int? EmergencyId { get; set; }
        public bool IsSuccess { get; set; }
    }
}