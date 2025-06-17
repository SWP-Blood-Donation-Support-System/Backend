using BloodDonationAPI.Entities;
using BloodDonationAPI.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BloodDonationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _searchService;

        public SearchController(ISearchService searchService)
        {
            _searchService = searchService;
        }        /// <summary>
        /// Tìm kiếm người hiến máu theo nhóm máu
        /// </summary>
        /// <remarks>
        /// API này cho phép tìm kiếm người hiến máu theo nhóm máu cụ thể.
        /// Chỉ Staff hoặc Admin mới có quyền truy cập.
        /// </remarks>
        /// <param name="bloodType">Nhóm máu (A+, A-, B+, B-, AB+, AB-, O+, O-)</param>
        /// <returns>Danh sách người hiến máu có nhóm máu tương thích, sắp xếp theo địa chỉ</returns>
        [HttpGet("donors/byBloodType")]
        [Authorize(Roles = "Staff,Admin")]public async Task<IActionResult> GetDonorsByBloodType([FromQuery] string bloodType)
        {
            try
            {                // Lấy thông tin người dùng hiện tại từ token
                var currentUser = User.Identity?.Name ?? string.Empty;
                var role = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value ?? string.Empty;

                // Validate input parameters
                if (string.IsNullOrWhiteSpace(bloodType))
                {
                    return BadRequest(new { message = "BloodType is required" });
                }

                // Chuẩn hóa bloodType
                var normalizedBloodType = bloodType.ToUpper().Trim();
                var validBloodTypes = new[] { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" };
                
                if (!validBloodTypes.Contains(normalizedBloodType))
                {
                    return BadRequest(new { message = "Invalid blood type. Must be A+, A-, B+, B-, AB+, AB-, O+, or O-" });
                }
                
                var donors = await _searchService.FindDonorsByBloodType(bloodType);
                
                return Ok(new { 
                    donors,
                    message = "Staff/Admin only API access successful",
                    currentUser = currentUser,
                    role = role
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }        /// <summary>
        /// Tìm kiếm yêu cầu cần máu theo nhóm máu
        /// </summary>
        /// <remarks>
        /// API này cho phép tìm kiếm các yêu cầu cần máu theo nhóm máu cụ thể.
        /// - Staff và Admin: có thể tìm kiếm không giới hạn (bloodType có thể để trống)
        /// - User và khách: bắt buộc phải cung cấp bloodType
        /// </remarks>
        /// <param name="bloodType">Nhóm máu (A+, A-, B+, B-, AB+, AB-, O+, O-)</param>
        /// <returns>Danh sách người cần máu có nhóm máu đã chỉ định</returns>
        [HttpGet("requests/byBloodType")]
        [AllowAnonymous]
        public async Task<IActionResult> GetEmergenciesByBloodType([FromQuery] string bloodType)
        {
            try
            {                // Kiểm tra người dùng hiện tại (nếu đã đăng nhập)
                bool isAuthenticated = User.Identity?.IsAuthenticated == true;
                string currentUser = User.Identity?.Name ?? string.Empty;
                string? role = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;
                bool isStaffOrAdmin = isAuthenticated && (role == "Staff" || role == "Admin");
                                     
                if (!isStaffOrAdmin && string.IsNullOrWhiteSpace(bloodType))
                {
                    return BadRequest(new { message = "BloodType is required" });
                }

                // Xử lý tìm kiếm theo bloodType hoặc tất cả
                if (!string.IsNullOrWhiteSpace(bloodType))
                {
                    // Chuẩn hóa bloodType
                    var normalizedBloodType = bloodType.ToUpper().Trim();
                    var validBloodTypes = new[] { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" };
                    
                    if (!validBloodTypes.Contains(normalizedBloodType))
                    {
                        return BadRequest(new { message = "Invalid blood type. Must be A+, A-, B+, B-, AB+, AB-, O+, or O-" });
                    }
                    
                    var emergencies = await _searchService.FindEmergenciesByBloodType(bloodType);
                    
                    return Ok(new { 
                        emergencies,
                        isAuthenticated,
                        currentUser = isAuthenticated ? currentUser : "Anonymous",
                        accessType = isAuthenticated ? "Authenticated user" : "Public access",
                        message = "Blood request search successful"
                    });
                }
                else if (isStaffOrAdmin) // Chỉ Staff/Admin mới có thể tìm tất cả
                {
                    var allEmergencies = await _searchService.FindAllEmergencies();
                      return Ok(new { 
                        emergencies = allEmergencies,
                        currentUser = currentUser ?? string.Empty,
                        role = role ?? string.Empty,
                        message = "Staff/Admin all emergencies access successful"
                    });
                }
                else
                {
                    // Đây là trường hợp không thể xảy ra do đã kiểm tra ở trên
                    return BadRequest(new { message = "Invalid request" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
    }
}