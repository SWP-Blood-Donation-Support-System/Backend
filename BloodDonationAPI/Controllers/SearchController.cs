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
        }

        /// <summary>
        /// Tìm kiếm người hiến máu theo nhóm máu
        /// </summary>
        /// <remarks>
        /// API này cho phép tìm kiếm người hiến máu theo nhóm máu cụ thể.
        /// </remarks>
        /// <param name="bloodType">Nhóm máu (A+, A-, B+, B-, AB+, AB-, O+, O-)</param>
        /// <returns>Danh sách người hiến máu có nhóm máu tương thích, sắp xếp theo địa chỉ</returns>
        [HttpGet("donors/byBloodType")]
        [AllowAnonymous]
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
        }

        /// <summary>
        /// Tìm kiếm yêu cầu cần máu theo nhóm máu
        /// </summary>
        /// <remarks>
        /// API này cho phép tìm kiếm các yêu cầu cần máu theo nhóm máu cụ thể.
        /// </remarks>
        /// <param name="bloodType">Nhóm máu (A+, A-, B+, B-, AB+, AB-, O+, O-)</param>
        /// <returns>Danh sách người cần máu có nhóm máu đã chỉ định</returns>
        [HttpGet("requests/byBloodType")]
        [AllowAnonymous]
        public async Task<IActionResult> GetEmergenciesByBloodType([FromQuery] string bloodType)
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

                var emergencies = await _searchService.FindEmergenciesByBloodType(bloodType);
                
                return Ok(new { emergencies });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
          // Hospital search endpoint removed as requested
    }
}