using BloodDonationAPI.DTO;
using BloodDonationAPI.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BloodDonationAPI.Controllers
{
    /// <summary>
    /// Controller quản lý danh sách người dùng dành cho Admin
    /// </summary>
    [Route("api/admin/users")]
    [ApiController]
    [Authorize(Roles = "Admin")] // Chỉ Admin mới có quyền truy cập
    public class AdminUserController : ControllerBase
    {
        private readonly IAdminUserService _adminUserService;
        private readonly ILogger<AdminUserController> _logger;

        public AdminUserController(IAdminUserService adminUserService, ILogger<AdminUserController> logger)
        {
            _adminUserService = adminUserService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách tất cả người dùng với tìm kiếm và phân trang
        /// </summary>
        /// <param name="request">Tham số tìm kiếm và phân trang</param>
        /// <returns>Danh sách người dùng và thông tin phân trang</returns>
        /// <remarks>
        /// API này cho phép admin:
        /// - Tìm kiếm người dùng theo username, email, role, fullname, bloodtype, status
        /// - Phân trang kết quả
        /// - Sắp xếp theo username
        /// 
        /// Ví dụ request:
        /// {
        ///   "username": "john",
        ///   "role": "User",
        ///   "page": 1,
        ///   "pageSize": 10
        /// }
        /// </remarks>
        [HttpGet]
        public async Task<IActionResult> GetUserList([FromQuery] UserSearchRequestDto request)
        {
            try
            {
                var result = await _adminUserService.GetUserListAsync(request);
                return Ok(new
                {
                    success = true,
                    message = "Lấy danh sách người dùng thành công",
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetUserList: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi server khi lấy danh sách người dùng",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một người dùng theo username
        /// </summary>
        /// <param name="username">Tên đăng nhập của người dùng</param>
        /// <returns>Thông tin chi tiết người dùng</returns>
        /// <remarks>
        /// API này trả về thông tin đầy đủ của người dùng bao gồm:
        /// - Thông tin cá nhân
        /// - Số lần đặt lịch hiến máu
        /// - Số lần hiến máu thành công
        /// - Trạng thái tài khoản
        /// </remarks>
        [HttpGet("{username}")]
        public async Task<IActionResult> GetUserByUsername(string username)
        {
            try
            {
                var user = await _adminUserService.GetUserByUsernameAsync(username);
                if (user == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"Không tìm thấy người dùng với username: {username}"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Lấy thông tin người dùng thành công",
                    data = user
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetUserByUsername: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi server khi lấy thông tin người dùng",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Cập nhật thông tin người dùng
        /// </summary>
        /// <param name="request">Thông tin cập nhật</param>
        /// <returns>Kết quả cập nhật</returns>
        /// <remarks>
        /// API này cho phép admin cập nhật thông tin người dùng:
        /// - Email, role, fullname, thông tin cá nhân
        /// - Trạng thái tài khoản
        /// - Nhóm máu
        /// 
        /// Lưu ý: Không thể cập nhật password qua API này
        /// </remarks>
        [HttpPut("update")]
        public async Task<IActionResult> UpdateUser([FromBody] AdminUpdateUserDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Dữ liệu không hợp lệ",
                        errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                    });
                }

                var result = await _adminUserService.UpdateUserAsync(request);
                if (!result)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"Không tìm thấy người dùng với username: {request.Username}"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Cập nhật thông tin người dùng thành công"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in UpdateUser: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi server khi cập nhật thông tin người dùng",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Thay đổi trạng thái người dùng (khóa/mở khóa tài khoản)
        /// </summary>
        /// <param name="request">Thông tin thay đổi trạng thái</param>
        /// <returns>Kết quả thay đổi trạng thái</returns>
        /// <remarks>
        /// API này cho phép admin:
        /// - Khóa tài khoản người dùng (status: "Inactive")
        /// - Mở khóa tài khoản (status: "Active")
        /// - Xóa tài khoản (status: "Deleted")
        /// 
        /// Các trạng thái có thể: "Active", "Inactive", "Deleted"
        /// </remarks>
        [HttpPut("status")]
        public async Task<IActionResult> ChangeUserStatus([FromBody] ChangeUserStatusDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Dữ liệu không hợp lệ",
                        errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                    });
                }

                var result = await _adminUserService.ChangeUserStatusAsync(request);
                if (!result)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"Không tìm thấy người dùng với username: {request.Username}"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = $"Thay đổi trạng thái người dùng {request.Username} thành công thành {request.NewStatus}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in ChangeUserStatus: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi server khi thay đổi trạng thái người dùng",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Xóa người dùng
        /// </summary>
        /// <param name="username">Tên đăng nhập của người dùng cần xóa</param>
        /// <returns>Kết quả xóa</returns>
        /// <remarks>
        /// API này thực hiện soft delete:
        /// - Nếu người dùng có dữ liệu liên quan (appointments, blogs, emergencies): chỉ đổi status thành "Deleted"
        /// - Nếu không có dữ liệu liên quan: xóa hoàn toàn khỏi database
        /// 
        /// Lưu ý: Chỉ nên sử dụng khi thực sự cần thiết
        /// </remarks>
        [HttpDelete("{username}")]
        public async Task<IActionResult> DeleteUser(string username)
        {
            try
            {
                var result = await _adminUserService.DeleteUserAsync(username);
                if (!result)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"Không tìm thấy người dùng với username: {username}"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = $"Xóa người dùng {username} thành công"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in DeleteUser: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi server khi xóa người dùng",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Lấy danh sách người dùng theo role
        /// </summary>
        /// <param name="role">Role cần lọc (Admin, User)</param>
        /// <param name="page">Số trang</param>
        /// <param name="pageSize">Kích thước trang</param>
        /// <returns>Danh sách người dùng theo role</returns>
        /// <remarks>
        /// API này lấy danh sách người dùng theo role cụ thể:
        /// - role = "Admin": Lấy tất cả admin
        /// - role = "User": Lấy tất cả user thường
        /// 
        /// Hỗ trợ phân trang để hiển thị hiệu quả
        /// </remarks>
        [HttpGet("role/{role}")]
        public async Task<IActionResult> GetUsersByRole(string role, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _adminUserService.GetUsersByRoleAsync(role, page, pageSize);
                return Ok(new
                {
                    success = true,
                    message = $"Lấy danh sách người dùng theo role {role} thành công",
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetUsersByRole: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi server khi lấy danh sách người dùng theo role",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Lấy danh sách người dùng theo trạng thái
        /// </summary>
        /// <param name="status">Trạng thái cần lọc (Active, Inactive, Deleted)</param>
        /// <param name="page">Số trang</param>
        /// <param name="pageSize">Kích thước trang</param>
        /// <returns>Danh sách người dùng theo trạng thái</returns>
        /// <remarks>
        /// API này lấy danh sách người dùng theo trạng thái:
        /// - status = "Active": Người dùng đang hoạt động
        /// - status = "Inactive": Người dùng bị khóa
        /// - status = "Deleted": Người dùng đã bị xóa
        /// </remarks>
        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetUsersByStatus(string status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _adminUserService.GetUsersByStatusAsync(status, page, pageSize);
                return Ok(new
                {
                    success = true,
                    message = $"Lấy danh sách người dùng theo trạng thái {status} thành công",
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetUsersByStatus: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi server khi lấy danh sách người dùng theo trạng thái",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Lấy tổng số người dùng trong hệ thống
        /// </summary>
        /// <returns>Tổng số người dùng</returns>
        /// <remarks>
        /// API này trả về tổng số người dùng đã đăng ký trong hệ thống
        /// </remarks>
        [HttpGet("count/total")]
        public async Task<IActionResult> GetTotalUserCount()
        {
            try
            {
                var count = await _adminUserService.GetTotalUserCountAsync();
                return Ok(new
                {
                    success = true,
                    message = "Lấy tổng số người dùng thành công",
                    data = new { totalUsers = count }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetTotalUserCount: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi server khi lấy tổng số người dùng",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Lấy thống kê tổng quan về người dùng
        /// </summary>
        /// <returns>Thống kê chi tiết về người dùng</returns>
        /// <remarks>
        /// API này trả về thống kê chi tiết bao gồm:
        /// - Tổng số người dùng
        /// - Số admin và user thường
        /// - Số người dùng theo trạng thái (Active, Inactive, Deleted)
        /// - Số người dùng có đặt lịch hiến máu
        /// - Số người dùng đã hiến máu thành công
        /// 
        /// Dùng để hiển thị dashboard cho admin
        /// </remarks>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetUserStatistics()
        {
            try
            {
                var statistics = await _adminUserService.GetUserStatisticsAsync();
                return Ok(new
                {
                    success = true,
                    message = "Lấy thống kê người dùng thành công",
                    data = statistics
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetUserStatistics: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi server khi lấy thống kê người dùng",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Tạo tài khoản Admin hoặc Staff
        /// </summary>
        /// <param name="request">Thông tin tài khoản cần tạo</param>
        /// <returns>Thông tin tài khoản đã tạo</returns>
        /// <remarks>
        /// API này cho phép admin tạo tài khoản cho:
        /// - Admin: Có tất cả quyền trong hệ thống
        /// - Staff: Có quyền hạn hỗ trợ quản lý
        /// 
        /// Lưu ý:
        /// - Username và email phải duy nhất
        /// - Role chỉ có thể là "Admin" hoặc "Staff"
        /// - Tài khoản được tạo sẽ có trạng thái "Active"
        /// - Mật khẩu nên được thay đổi sau lần đăng nhập đầu tiên
        /// 
        /// Ví dụ request:
        /// {
        ///   "username": "admin_staff_01",
        ///   "password": "TempPassword123",
        ///   "email": "staff@example.com",
        ///   "role": "Staff",
        ///   "fullName": "Nguyễn Văn A",
        ///   "phone": "0123456789"
        /// }
        /// </remarks>
        [HttpPost("create-account")]
        public async Task<IActionResult> CreateAdminAccount([FromBody] CreateAdminAccountDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Dữ liệu không hợp lệ",
                        errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                    });
                }

                var result = await _adminUserService.CreateAdminAccountAsync(request);
                
                if (result == null)
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "Không thể tạo tài khoản"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = $"Tạo tài khoản {request.Role} thành công",
                    data = result
                });
            }
            catch (InvalidOperationException ex)
            {
                // Lỗi business logic (username/email đã tồn tại)
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in CreateAdminAccount: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi server khi tạo tài khoản",
                    error = ex.Message
                });
            }
        }
    }
} 