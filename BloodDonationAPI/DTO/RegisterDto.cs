using System.ComponentModel.DataAnnotations;

namespace BloodDonationAPI.DTO
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập.")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        public string Password { get; set; }

        public string? Email { get; set; }

       

        public string? FullName { get; set; }

        public DateOnly? DateOfBirth { get; set; }

        public string? Gender { get; set; }

        public string? Phone { get; set; }

        public string? Address { get; set; }

       

        public int? BloodTypeId { get; set; }
    }
}
