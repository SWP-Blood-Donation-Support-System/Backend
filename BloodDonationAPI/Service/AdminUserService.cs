using BloodDonationAPI.DTO;
using BloodDonationAPI.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BloodDonationAPI.Service.Impl
{
    public class AdminUserService : IAdminUserService
    {
        private readonly BloodDonationSystemContext _context;
        private readonly ILogger<AdminUserService> _logger;

        public AdminUserService(BloodDonationSystemContext context, ILogger<AdminUserService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<UserListResponseDto> GetUserListAsync(UserSearchRequestDto request)
        {
            try
            {
                var query = _context.Users.AsQueryable();

                // Áp dụng các filter
                if (!string.IsNullOrEmpty(request.Username))
                {
                    query = query.Where(u => u.Username.Contains(request.Username));
                }

                if (!string.IsNullOrEmpty(request.Email))
                {
                    query = query.Where(u => u.Email != null && u.Email.Contains(request.Email));
                }

                if (!string.IsNullOrEmpty(request.Role))
                {
                    query = query.Where(u => u.Role == request.Role);
                }

                if (!string.IsNullOrEmpty(request.FullName))
                {
                    query = query.Where(u => u.FullName != null && u.FullName.Contains(request.FullName));
                }

                if (!string.IsNullOrEmpty(request.BloodType))
                {
                    query = query.Where(u => u.BloodType == request.BloodType);
                }

                if (!string.IsNullOrEmpty(request.ProfileStatus))
                {
                    query = query.Where(u => u.ProfileStatus == request.ProfileStatus);
                }

                // Đếm tổng số bản ghi
                var totalCount = await query.CountAsync();

                // Áp dụng phân trang
                var users = await query
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(u => new UserListDto
                    {
                        Username = u.Username,
                        Email = u.Email,
                        Role = u.Role,
                        FullName = u.FullName,
                        DateOfBirth = u.DateOfBirth,
                        Gender = u.Gender,
                        Phone = u.Phone,
                        Address = u.Address,
                        ProfileStatus = u.ProfileStatus,
                        BloodType = u.BloodType,
                        TotalAppointments = u.AppointmentRecords.Count,
                        TotalDonations = u.AppointmentRecords.Count(a => a.Status == "Completed"),
                        CreatedDate = DateTime.Now, // Có thể thêm trường CreatedDate vào User entity
                        LastLoginDate = DateTime.Now // Có thể thêm trường LastLoginDate vào User entity
                    })
                    .OrderBy(u => u.Username)
                    .ToListAsync();

                return new UserListResponseDto
                {
                    Users = users,
                    TotalCount = totalCount,
                    Page = request.Page,
                    PageSize = request.PageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetUserListAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<UserListDto?> GetUserByUsernameAsync(string username)
        {
            try
            {
                var user = await _context.Users
                    .Where(u => u.Username == username)
                    .Select(u => new UserListDto
                    {
                        Username = u.Username,
                        Email = u.Email,
                        Role = u.Role,
                        FullName = u.FullName,
                        DateOfBirth = u.DateOfBirth,
                        Gender = u.Gender,
                        Phone = u.Phone,
                        Address = u.Address,
                        ProfileStatus = u.ProfileStatus,
                        BloodType = u.BloodType,
                        TotalAppointments = u.AppointmentRecords.Count,
                        TotalDonations = u.AppointmentRecords.Count(a => a.Status == "Completed"),
                        CreatedDate = DateTime.Now,
                        LastLoginDate = DateTime.Now
                    })
                    .FirstOrDefaultAsync();

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetUserByUsernameAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> UpdateUserAsync(AdminUpdateUserDto request)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == request.Username);

                if (user == null)
                {
                    return false;
                }

                // Cập nhật thông tin
                user.Email = request.Email;
                user.Role = request.Role;
                user.FullName = request.FullName;
                user.DateOfBirth = request.DateOfBirth;
                user.Gender = request.Gender;
                user.Phone = request.Phone;
                user.Address = request.Address;
                user.ProfileStatus = request.ProfileStatus;
                user.BloodType = request.BloodType;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in UpdateUserAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> ChangeUserStatusAsync(ChangeUserStatusDto request)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == request.Username);

                if (user == null)
                {
                    return false;
                }

                user.ProfileStatus = request.NewStatus;
                await _context.SaveChangesAsync();

                // Có thể thêm log hoặc notification về việc thay đổi trạng thái
                _logger.LogInformation($"User {request.Username} status changed to {request.NewStatus}. Reason: {request.Reason}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in ChangeUserStatusAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteUserAsync(string username)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == username);

                if (user == null)
                {
                    return false;
                }

                // Kiểm tra xem user có dữ liệu liên quan không
                var hasAppointments = await _context.AppointmentRecords
                    .AnyAsync(a => a.Username == username);

                var hasBlogs = await _context.Blogs
                    .AnyAsync(b => b.Username == username);

                var hasEmergencies = await _context.Emergencies
                    .AnyAsync(e => e.Username == username);

                if (hasAppointments || hasBlogs || hasEmergencies)
                {
                    // Nếu có dữ liệu liên quan, chỉ thay đổi trạng thái thành "Deleted"
                    user.ProfileStatus = "Deleted";
                }
                else
                {
                    // Nếu không có dữ liệu liên quan, xóa hoàn toàn
                    _context.Users.Remove(user);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in DeleteUserAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<UserListResponseDto> GetUsersByRoleAsync(string role, int page = 1, int pageSize = 10)
        {
            try
            {
                var query = _context.Users.Where(u => u.Role == role);

                var totalCount = await query.CountAsync();

                var users = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(u => new UserListDto
                    {
                        Username = u.Username,
                        Email = u.Email,
                        Role = u.Role,
                        FullName = u.FullName,
                        DateOfBirth = u.DateOfBirth,
                        Gender = u.Gender,
                        Phone = u.Phone,
                        Address = u.Address,
                        ProfileStatus = u.ProfileStatus,
                        BloodType = u.BloodType,
                        TotalAppointments = u.AppointmentRecords.Count,
                        TotalDonations = u.AppointmentRecords.Count(a => a.Status == "Completed"),
                        CreatedDate = DateTime.Now,
                        LastLoginDate = DateTime.Now
                    })
                    .OrderBy(u => u.Username)
                    .ToListAsync();

                return new UserListResponseDto
                {
                    Users = users,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetUsersByRoleAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<UserListResponseDto> GetUsersByStatusAsync(string status, int page = 1, int pageSize = 10)
        {
            try
            {
                var query = _context.Users.Where(u => u.ProfileStatus == status);

                var totalCount = await query.CountAsync();

                var users = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(u => new UserListDto
                    {
                        Username = u.Username,
                        Email = u.Email,
                        Role = u.Role,
                        FullName = u.FullName,
                        DateOfBirth = u.DateOfBirth,
                        Gender = u.Gender,
                        Phone = u.Phone,
                        Address = u.Address,
                        ProfileStatus = u.ProfileStatus,
                        BloodType = u.BloodType,
                        TotalAppointments = u.AppointmentRecords.Count,
                        TotalDonations = u.AppointmentRecords.Count(a => a.Status == "Completed"),
                        CreatedDate = DateTime.Now,
                        LastLoginDate = DateTime.Now
                    })
                    .OrderBy(u => u.Username)
                    .ToListAsync();

                return new UserListResponseDto
                {
                    Users = users,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetUsersByStatusAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<int> GetTotalUserCountAsync()
        {
            try
            {
                return await _context.Users.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetTotalUserCountAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<Dictionary<string, int>> GetUserStatisticsAsync()
        {
            try
            {
                var statistics = new Dictionary<string, int>();

                // Tổng số user
                statistics["TotalUsers"] = await _context.Users.CountAsync();

                // Số user theo role
                statistics["AdminUsers"] = await _context.Users.CountAsync(u => u.Role == "Admin");
                statistics["RegularUsers"] = await _context.Users.CountAsync(u => u.Role == "User");

                // Số user theo trạng thái
                statistics["ActiveUsers"] = await _context.Users.CountAsync(u => u.ProfileStatus == "Active");
                statistics["InactiveUsers"] = await _context.Users.CountAsync(u => u.ProfileStatus == "Inactive");
                statistics["DeletedUsers"] = await _context.Users.CountAsync(u => u.ProfileStatus == "Deleted");

                // Số user có đặt lịch hiến máu
                statistics["UsersWithAppointments"] = await _context.Users
                    .CountAsync(u => u.AppointmentRecords.Any());

                // Số user đã hiến máu thành công
                statistics["UsersWithDonations"] = await _context.Users
                    .CountAsync(u => u.AppointmentRecords.Any(a => a.Status == "Completed"));

                return statistics;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetUserStatisticsAsync: {ex.Message}");
                throw;
            }
        }
    }
} 