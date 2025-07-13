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

                // 🆕 Filter theo ProfileStatus (giữ nguyên)
                if (!string.IsNullOrEmpty(request.ProfileStatus))
                {
                    query = query.Where(u => u.ProfileStatus == request.ProfileStatus);
                }

                // 🆕 Filter theo UserStatus (thêm mới)
                if (!string.IsNullOrEmpty(request.UserStatus))
                {
                    query = query.Where(u => u.UserStatus == request.UserStatus);
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
                        ProfileStatus = u.ProfileStatus, // Giữ nguyên
                        UserStatus = u.UserStatus, // Thêm mới
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
                        ProfileStatus = u.ProfileStatus, // Giữ nguyên
                        UserStatus = u.UserStatus, // Thêm mới
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
                user.ProfileStatus = request.ProfileStatus; // Giữ nguyên
                user.UserStatus = request.UserStatus; // Thêm mới
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

                // 🆕 Cập nhật UserStatus mới (cho workflow admin)
                user.UserStatus = request.NewStatus;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"User {request.Username} UserStatus changed to {request.NewStatus}. Reason: {request.Reason}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in ChangeUserStatusAsync: {ex.Message}");
                throw;
            }
        }

        // 🆕 Thêm method mới để thay đổi ProfileStatus
        public async Task<bool> ChangeProfileStatusAsync(ChangeProfileStatusDto request)
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

                _logger.LogInformation($"User {request.Username} ProfileStatus changed to {request.NewStatus}. Reason: {request.Reason}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in ChangeProfileStatusAsync: {ex.Message}");
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
                        ProfileStatus = u.ProfileStatus, // Giữ nguyên
                        UserStatus = u.UserStatus, // Thêm mới
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

        // 🆕 Method cho ProfileStatus (giữ nguyên)
        public async Task<UserListResponseDto> GetUsersByProfileStatusAsync(string status, int page = 1, int pageSize = 10)
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
                        UserStatus = u.UserStatus,
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
                _logger.LogError($"Error in GetUsersByProfileStatusAsync: {ex.Message}");
                throw;
            }
        }

        // 🆕 Method cho UserStatus (mới)
        public async Task<UserListResponseDto> GetUsersByUserStatusAsync(string status, int page = 1, int pageSize = 10)
        {
            try
            {
                var query = _context.Users.Where(u => u.UserStatus == status);

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
                        UserStatus = u.UserStatus,
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
                _logger.LogError($"Error in GetUsersByUserStatusAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<int> GetTotalUserCountAsync()
        {
            try
            {
                // Đếm tổng số user trong hệ thống
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
                statistics["StaffUsers"] = await _context.Users.CountAsync(u => u.Role == "Staff");
                statistics["RegularUsers"] = await _context.Users.CountAsync(u => u.Role == "User");

                // 🆕 Thống kê theo ProfileStatus (chỉ 2 trạng thái)
                statistics["ReadyToDonate"] = await _context.Users.CountAsync(u => u.ProfileStatus == "Sẵn sàng hiến máu");
                statistics["Resting"] = await _context.Users.CountAsync(u => u.ProfileStatus == "Đang nghỉ ngơi");

                // 🆕 Thống kê theo UserStatus (chỉ Active và Inactive)
                statistics["ActiveUsers"] = await _context.Users.CountAsync(u => u.UserStatus == "Active");
                statistics["InactiveUsers"] = await _context.Users.CountAsync(u => u.UserStatus == "Inactive");

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

        public async Task<CreateAccountResponseDto?> CreateAdminAccountAsync(CreateAdminAccountDto request)
        {
            try
            {
                // Kiểm tra xem username đã tồn tại chưa
                var existingUserByUsername = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == request.Username);

                if (existingUserByUsername != null)
                {
                    throw new InvalidOperationException("Tên đăng nhập đã tồn tại.");
                }

                // Kiểm tra xem email đã tồn tại chưa
                var existingUserByEmail = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == request.Email);

                if (existingUserByEmail != null)
                {
                    throw new InvalidOperationException("Email đã được sử dụng để đăng ký tài khoản khác.");
                }

                // Tạo user mới
                var newUser = new User
                {
                    Username = request.Username,
                    Password = request.Password,
                    Email = request.Email,
                    Role = request.Role,
                    FullName = request.FullName,
                    DateOfBirth = request.DateOfBirth,
                    Gender = request.Gender,
                    Phone = request.Phone,
                    Address = request.Address,
                    BloodType = request.BloodType,
                    ProfileStatus = "Sẵn sàng hiến máu", // 🆕 Sửa từ "Active" thành "Sẵn sàng hiến máu"
                    UserStatus = "Active"
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Admin account created successfully: {request.Username} with role {request.Role}");

                // Trả về thông tin account đã tạo
                return new CreateAccountResponseDto
                {
                    Username = newUser.Username,
                    Email = newUser.Email!,
                    Role = newUser.Role!,
                    FullName = newUser.FullName,
                    ProfileStatus = newUser.ProfileStatus!,
                    UserStatus = newUser.UserStatus!,
                    CreatedDate = DateTime.Now
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in CreateAdminAccountAsync: {ex.Message}");
                throw;
            }
        }
    }
} 