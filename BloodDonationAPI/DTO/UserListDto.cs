using System.ComponentModel.DataAnnotations;

namespace BloodDonationAPI.DTO
{
    public class UserListDto
    {
        public string Username { get; set; } = null!;
        
        public string? Email { get; set; }
        
        public string? Role { get; set; }
        
        public string? FullName { get; set; }
        
        public DateOnly? DateOfBirth { get; set; }
        
        public string? Gender { get; set; }
        
        public string? Phone { get; set; }
        
        public string? Address { get; set; }
        
        public string? ProfileStatus { get; set; }
        
        public string? BloodType { get; set; }
        
        // Thêm các thông tin thống kê nếu cần
        public int TotalAppointments { get; set; }
        
        public int TotalDonations { get; set; }
        
        public DateTime? LastLoginDate { get; set; }
        
        public DateTime CreatedDate { get; set; }
    }

    // DTO cho request tìm kiếm/lọc người dùng
    public class UserSearchRequestDto
    {
        public string? Username { get; set; }
        
        public string? Email { get; set; }
        
        public string? Role { get; set; }
        
        public string? FullName { get; set; }
        
        public string? BloodType { get; set; }
        
        public string? ProfileStatus { get; set; }
        
        public int Page { get; set; } = 1;
        
        public int PageSize { get; set; } = 10;
    }

    // DTO cho response danh sách người dùng có phân trang
    public class UserListResponseDto
    {
        public List<UserListDto> Users { get; set; } = new List<UserListDto>();
        
        public int TotalCount { get; set; }
        
        public int Page { get; set; }
        
        public int PageSize { get; set; }
        
        public int TotalPages { get; set; }
    }

    // DTO cho việc cập nhật thông tin người dùng bởi admin
    public class AdminUpdateUserDto
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập.")]
        public string Username { get; set; } = null!;
        
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string? Email { get; set; }
        
        public string? Role { get; set; }
        
        public string? FullName { get; set; }
        
        public DateOnly? DateOfBirth { get; set; }
        
        public string? Gender { get; set; }
        
        public string? Phone { get; set; }
        
        public string? Address { get; set; }
        
        public string? ProfileStatus { get; set; }
        
        public string? BloodType { get; set; }
    }

    // DTO cho việc thay đổi trạng thái người dùng
    public class ChangeUserStatusDto
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập.")]
        public string Username { get; set; } = null!;
        
        [Required(ErrorMessage = "Vui lòng chọn trạng thái mới.")]
        public string NewStatus { get; set; } = null!;
        
        public string? Reason { get; set; }
    }
}
