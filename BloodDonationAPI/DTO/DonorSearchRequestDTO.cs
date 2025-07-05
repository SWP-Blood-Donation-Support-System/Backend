using System.ComponentModel.DataAnnotations;

namespace BloodDonationAPI.DTO
{
    public class DonorSearchRequestDTO
    {
        public double Lat { get; set; }

        public double Lng { get; set; }

        public double Radius { get; set; } = 10.0; // Mặc định bán kính tìm kiếm 10km

        [Required]
        public string BloodType { get; set; }
    }
}
