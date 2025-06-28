namespace BloodDonationAPI.DTO
{
    public class RegisterEmergencyDto
    {
        public string BloodType { get; set; } = null!;
        public int? RequiredUnits { get; set; }
        public int? HospitalId { get; set; }
        public string? EmergencyMedical { get; set; }
        public string? EmergencyImage { get; set; }
        public DateOnly? EndDate { get; set; }
    }
} 