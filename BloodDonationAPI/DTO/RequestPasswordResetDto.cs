using System.ComponentModel.DataAnnotations;

namespace BloodDonationAPI.DTO
{
    public class RequestPasswordResetDto
    {
        [Required(ErrorMessage = "Vui lòng nhập email hoặc tên đăng nhập.")]
        public string EmailOrUsername { get; set; } = null!;
    }
} 