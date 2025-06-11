using BloodDonationAPI.DTOs;
using BloodDonationAPI.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodDonationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DonorsController : ControllerBase
    {
        private readonly IDonorSearchService _donorSearchService;

        public DonorsController(IDonorSearchService donorSearchService)
        {
            _donorSearchService = donorSearchService;
        }        /// <summary>
        /// Tìm kiếm người hiến máu gần đây theo vị trí
        /// </summary>
        /// <param name="bloodType">Nhóm máu cần tìm (A+, A-, B+, B-, AB+, AB-, O+, O-)</param>
        /// <param name="lat">Vĩ độ (-90 đến 90)</param>
        /// <param name="lng">Kinh độ (-180 đến 180)</param>
        /// <param name="radius">Bán kính tìm kiếm (km, tối đa 1000km)</param>
        /// <returns>Danh sách người hiến máu gần đây sắp xếp theo khoảng cách</returns>
        [HttpGet("nearby")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> GetNearbyDonors(
            [FromQuery] string bloodType,
            [FromQuery] double lat,
            [FromQuery] double lng,
            [FromQuery] double radius)
        {
            try
            {
                // Validate input parameters
                if (string.IsNullOrWhiteSpace(bloodType))
                {
                    return BadRequest(new { message = "BloodType is required" });
                }

                if (lat < -90 || lat > 90)
                {
                    return BadRequest(new { message = "Invalid latitude. Must be between -90 and 90" });
                }

                if (lng < -180 || lng > 180)
                {
                    return BadRequest(new { message = "Invalid longitude. Must be between -180 and 180" });
                }

                if (radius <= 0 || radius > 1000)
                {
                    return BadRequest(new { message = "Invalid radius. Must be between 0.1 and 1000 km" });
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
                    BloodType = normalizedBloodType,
                    Lat = lat,
                    Lng = lng,
                    Radius = radius
                };

                var result = await _donorSearchService.FindNearbyDonorsAsync(request);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
    }
}