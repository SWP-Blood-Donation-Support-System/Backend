using System.ComponentModel.DataAnnotations;

namespace BloodDonationAPI.DTO
{
    public class GoogleLoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string GoogleToken { get; set; } = string.Empty;
    }

    public class GoogleUserInfo
    {
        public string Sub { get; set; } = string.Empty; // Google user ID
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Picture { get; set; } = string.Empty;
        public bool EmailVerified { get; set; }
        public string GivenName { get; set; } = string.Empty;
        public string FamilyName { get; set; } = string.Empty;
    }

    public class GoogleLoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public bool IsNewUser { get; set; }
        public string Message { get; set; } = string.Empty;
        public UserInfo User { get; set; } = new UserInfo();
    }

    public class UserInfo
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateOnly? DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string BloodType { get; set; } = string.Empty;
        public string ProfileStatus { get; set; } = string.Empty;
    }
}
