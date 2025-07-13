using BloodDonationAPI.DTO;

namespace BloodDonationAPI.Service
{
    public interface IAdminUserService
    {
        Task<UserListResponseDto> GetUserListAsync(UserSearchRequestDto request);
        Task<UserListDto?> GetUserByUsernameAsync(string username);
        Task<bool> UpdateUserAsync(AdminUpdateUserDto request);
        Task<bool> ChangeUserStatusAsync(ChangeUserStatusDto request);
        Task<bool> ChangeProfileStatusAsync(ChangeProfileStatusDto request); // 🆕 Thêm mới
        Task<UserListResponseDto> GetUsersByRoleAsync(string role, int page = 1, int pageSize = 10);
        Task<UserListResponseDto> GetUsersByProfileStatusAsync(string status, int page = 1, int pageSize = 10); // 🆕 Giữ nguyên
        Task<UserListResponseDto> GetUsersByUserStatusAsync(string status, int page = 1, int pageSize = 10); // 🆕 Thêm mới
        Task<int> GetTotalUserCountAsync();
        Task<Dictionary<string, int>> GetUserStatisticsAsync();
        Task<CreateAccountResponseDto?> CreateAdminAccountAsync(CreateAdminAccountDto request);
    }
} 