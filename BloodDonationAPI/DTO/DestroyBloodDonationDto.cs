using BloodDonationAPI.Entities;

namespace BloodDonationAPI.DTO
{
    public class DestroyBloodDonationDto
    {
        public int BloodDetailID { get; set; }
        public string? ReasonCode { get; set; } 
        public string? CustomNote { get; set; }
    }
}
