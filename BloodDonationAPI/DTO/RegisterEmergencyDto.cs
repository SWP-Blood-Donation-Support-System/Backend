namespace BloodDonationAPI.DTO
{
    public class RegisterEmergencyDto
    {
        public string BloodType { get; set; } = null!;
        public int? RequiredUnits { get; set; }
        public int? HospitalId { get; set; }
    }
} 