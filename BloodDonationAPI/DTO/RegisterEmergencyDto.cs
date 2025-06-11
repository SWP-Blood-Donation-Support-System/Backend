namespace BloodDonationAPI.DTO
{
    public class RegisterEmergencyDto
    {
        public string BloodType { get; set; } = null!;
        public string? EmergencyNote { get; set; }
        public int? RequiredUnits { get; set; }
        public int? HospitalId { get; set; }
    }
} 