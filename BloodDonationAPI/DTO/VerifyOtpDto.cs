using System.ComponentModel.DataAnnotations;

namespace BloodDonationAPI.DTO
{
    public class VerifyOtpDto
    {
        [Required(ErrorMessage = "Vui lòng nhập mã OTP.")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã OTP phải có đúng 6 số.")]
        public string Otp { get; set; } = null!;
    }
} 