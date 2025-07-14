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
        
        [RegularExpression("^(Sẵn sàng hiến máu|Đang nghỉ ngơi|Không sẵn sàng)$", ErrorMessage = "ProfileStatus chỉ có thể là 'Sẵn sàng hiến máu', 'Đang nghỉ ngơi', hoặc 'Không sẵn sàng'.")]
        public string? ProfileStatus { get; set; } // Chỉ 3 trạng thái
        
        [RegularExpression("^(Active|Inactive)$", ErrorMessage = "UserStatus chỉ có thể là Active hoặc Inactive.")]
        public string? UserStatus { get; set; } // Chỉ Active và Inactive
        
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
        
        public string? ProfileStatus { get; set; } // Giữ nguyên
        
        public string? UserStatus { get; set; } // Thêm mới
        
        public int Page { get; set; } = 1;
        
        public int PageSize { get; set; } = 10;
    }

    // 🆕 DTO cho response danh sách người dùng có phân trang
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
        
        [RegularExpression("^(Sẵn sàng hiến máu|Đang nghỉ ngơi|Không sẵn sàng)$", ErrorMessage = "ProfileStatus chỉ có thể là 'Sẵn sàng hiến máu', 'Đang nghỉ ngơi', hoặc 'Không sẵn sàng'.")]
        public string? ProfileStatus { get; set; } // Chỉ 3 trạng thái
        
        [RegularExpression("^(Active|Inactive)$", ErrorMessage = "UserStatus chỉ có thể là Active hoặc Inactive.")]
        public string? UserStatus { get; set; } // Thêm mới
        
        public string? BloodType { get; set; }
    }

    // DTO cho việc thay đổi UserStatus (mới)
    public class ChangeUserStatusDto
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập.")]
        public string Username { get; set; } = null!;
        
        [Required(ErrorMessage = "Vui lòng chọn UserStatus mới.")]
        [RegularExpression("^(Active|Inactive)$", ErrorMessage = "UserStatus chỉ có thể là Active hoặc Inactive.")]
        public string NewStatus { get; set; } = null!;
        
        public string? Reason { get; set; }
    }

    // 🆕 DTO cho việc thay đổi ProfileStatus (giữ nguyên)
    public class ChangeProfileStatusDto
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập.")]
        public string Username { get; set; } = null!;
        
        [Required(ErrorMessage = "Vui lòng chọn ProfileStatus mới.")]
        [RegularExpression("^(Sẵn sàng hiến máu|Đang nghỉ ngơi|Không sẵn sàng)$", ErrorMessage = "ProfileStatus chỉ có thể là 'Sẵn sàng hiến máu', 'Đang nghỉ ngơi', hoặc 'Không sẵn sàng'.")]
        public string NewStatus { get; set; } = null!;
        
        public string? Reason { get; set; }
    }

    // DTO cho việc tạo account admin/staff bởi admin
    public class CreateAdminAccountDto
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Tên đăng nhập phải từ 3 đến 50 ký tự.")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6 đến 100 ký tự.")]
        public string Password { get; set; } = null!;
        
        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng chọn vai trò.")]
        [RegularExpression("^(Admin|Staff)$", ErrorMessage = "Vai trò chỉ có thể là Admin hoặc Staff.")]
        public string Role { get; set; } = null!;

        [StringLength(100, ErrorMessage = "Họ và tên không được vượt quá 100 ký tự.")]
        public string? FullName { get; set; }

        public DateOnly? DateOfBirth { get; set; }

        [RegularExpression("^(Nam|Nữ)$", ErrorMessage = "Giới tính chỉ có thể là Nam hoặc Nữ.")]
        public string? Gender { get; set; }
        
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Số điện thoại phải có đúng 10 số.")]
        public string? Phone { get; set; }

        [StringLength(200, ErrorMessage = "Địa chỉ không được vượt quá 200 ký tự.")]
        public string? Address { get; set; }

        [RegularExpression("^(A|B|AB|O)[+-]$", ErrorMessage = "Nhóm máu không hợp lệ. Ví dụ: A+, B-, AB+, O-.")]
        public string? BloodType { get; set; }
    }

    // DTO cho response khi tạo account thành công
    public class CreateAccountResponseDto
    {
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Role { get; set; } = null!;
        public string? FullName { get; set; }
        public string ProfileStatus { get; set; } = null!; // Giữ nguyên
        public string UserStatus { get; set; } = null!; // Thêm mới
        public DateTime CreatedDate { get; set; }
    }
}
