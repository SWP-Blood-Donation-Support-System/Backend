namespace BloodDonationAPI.DTO
{
    public class CertificateDto
    {
        public int CertificateId { get; set; }
        public int AppointmentId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public DateOnly DateOfBirth { get; set; }
        public string Address { get; set; } = string.Empty;
        public string HospitalName { get; set; } = string.Empty;
        public int BloodAmount { get; set; }
        public DateOnly DonationDate { get; set; }
        public string CertificateCode { get; set; } =string.Empty;
        public DateOnly IssueDate { get; set; }
        
    }
}
