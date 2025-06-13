using BloodDonationAPI.DTO;
using BloodDonationAPI.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BloodDonationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmergencyController : ControllerBase
    {
        private readonly IEmergencyService _emergencyService;
        private readonly ILogger<EmergencyController> _logger;

        public EmergencyController(IEmergencyService emergencyService, ILogger<EmergencyController> logger)
        {
            _emergencyService = emergencyService;
            _logger = logger;
        }
        /// <summary>
        /// Dùng để đăng ký đơn khẩn cấp cần đăng nhập 
        /// </summary>
        
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("RegisterEmergency")]
        [Authorize(Roles = "User,Staff,Admin")]
        public async Task<IActionResult> RegisterEmergency([FromBody] RegisterEmergencyDto dto)
        {
            try
            {
                var username = User.FindFirst(ClaimTypes.Name)?.Value;
                if (username == null)
                    return Unauthorized(new { message = "User not authenticated." });

                var role = User.FindFirst(ClaimTypes.Role)?.Value;
                if (role == null)
                    return Unauthorized(new { message = "User role not found." });

                if (string.IsNullOrEmpty(dto.BloodType))
                    return BadRequest(new { message = "Blood type is required." });

                var result = await _emergencyService.RegisterEmergency(username, role, dto);
                if (result == "Emergency registration successful.")
                    return Ok(new { message = result });

                return BadRequest(new { message = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering emergency");
                return StatusCode(500, new { message = "An error occurred while processing your request.", error = ex.Message });
            }
        }
        
        /// <summary>
        /// Dùng để hiển thị danh sách các đơn khẩn cấp 
        /// </summary>
        
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpGet("GetEmergencies")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> GetEmergencies()
        {
            try
            {
                var emergencies = await _emergencyService.GetEmergencies();
                return Ok(emergencies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting emergencies");
                return StatusCode(500, new { message = "An error occurred while processing your request.", error = ex.Message });
            }
        }

        /// <summary>
        /// Dùng để xét duyệt đơn đăng ký khẩn cấp, nếu admin or staff tạo đơn thì mặc định đã xét duyệt
        /// </summary>
        /// <remarks>
        /// Cần nhập đúng chuỗi "Đã xét duyệt" or "Từ chối"
        /// </remarks>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPut("UpdateStatus/{emergencyId}")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> UpdateEmergencyStatus(int emergencyId, [FromBody] string status)
        {
            try
            {
                if (string.IsNullOrEmpty(status))
                    return BadRequest(new { message = "Status is required." });

                var result = await _emergencyService.UpdateEmergencyStatus(emergencyId, status);
                if (result == "Emergency status updated successfully.")
                    return Ok(new { message = result });

                return BadRequest(new { message = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating emergency status");
                return StatusCode(500, new { message = "An error occurred while processing your request.", error = ex.Message });
            }
        }
    }
} 