using System.ComponentModel.DataAnnotations;

namespace BloodDonationAPI.DTO
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập.")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        public string Password { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string? Email { get; set; }

        public string? FullName { get; set; }

        public DateOnly? DateOfBirth { get; set; }

        public string? Gender { get; set; }
        
        // Thêm validation cho Phone
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        public string? Phone { get; set; }

        public string? Address { get; set; }

        public string? BloodTypeId { get; set; }
    }
}
