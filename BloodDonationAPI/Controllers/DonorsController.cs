using BloodDonationAPI.DTOs;
using BloodDonationAPI.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodDonationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Keep general authorization for the controller
    public class DonorsController : ControllerBase
    {
        private readonly IDonorSearchService _donorSearchService;

        public DonorsController(IDonorSearchService donorSearchService)
        {
            _donorSearchService = donorSearchService;
        }        /// <summary>
        /// Tìm kiếm người hiến máu theo nhóm máu
        /// </summary>
        /// <param name="bloodType">Nhóm máu cần tìm (A+, A-, B+, B-, AB+, AB-, O+, O-)</param>
        /// <returns>Danh sách người hiến máu có nhóm máu phù hợp</returns>
        [HttpGet("findByBloodType")]
        [AllowAnonymous] // Add this to allow anonymous access to this specific endpoint
        public async Task<IActionResult> GetDonorsByBloodType(
            [FromQuery] string bloodType)
        {            try
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

                var request = new DonorSearchRequest
                {
                    BloodType = normalizedBloodType
                };                // Sử dụng phương thức tìm theo nhóm máu trong phạm vi Việt Nam, sắp xếp theo khoảng cách
                var result = await _donorSearchService.FindDonorsInHCMByBloodTypeAsync(request);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
    }
}