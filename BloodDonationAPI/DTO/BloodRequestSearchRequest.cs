using System.ComponentModel.DataAnnotations;

namespace BloodDonationAPI.DTO
{
    public class BloodRequestSearchRequest
    {
        [Required]
        public double Lat { get; set; }

        [Required]
        public double Lng { get; set; }

        [Required]
        public double Radius { get; set; } // Bán kính tìm kiếm (km)

        public string? BloodType { get; set; } // Có thể null nếu tìm tất cả các nhóm máu
    }
}
