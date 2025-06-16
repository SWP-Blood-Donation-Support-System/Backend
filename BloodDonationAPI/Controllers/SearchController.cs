using BloodDonationAPI.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace BloodDonationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
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
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> GetDonorsByBloodType([FromQuery] string bloodType)
        {
            try
            {
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
                
                return Ok(new { donors });
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
        /// - User: bắt buộc phải cung cấp bloodType
        /// </remarks>
        /// <param name="bloodType">Nhóm máu (A+, A-, B+, B-, AB+, AB-, O+, O-)</param>
        /// <returns>Danh sách người cần máu có nhóm máu đã chỉ định</returns>
        [HttpGet("requests/byBloodType")]
        [Authorize]
        public async Task<IActionResult> GetEmergenciesByBloodType([FromQuery] string bloodType)
        {
            try
            {
                // Lấy role của người dùng từ claims
                var userRole = User.Claims.FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;
                
                // Nếu là User thì bắt buộc phải có bloodType
                if ((userRole == "User" || string.IsNullOrEmpty(userRole)) && string.IsNullOrWhiteSpace(bloodType))
                {
                    return BadRequest(new { message = "BloodType is required for regular users" });
                }

                // Staff/Admin không bắt buộc phải có bloodType
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
                    return Ok(new { emergencies });
                }
                else
                {
                    // Staff/Admin có thể tìm không giới hạn
                    var allEmergencies = await _searchService.FindAllEmergencies();
                    return Ok(new { emergencies = allEmergencies });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
          // Hospital search endpoint removed as requested
    }
}