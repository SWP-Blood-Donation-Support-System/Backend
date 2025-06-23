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

        /// <summary>
        /// So sánh lượng máu trong kho với đơn khẩn cấp, trả về trạng thái đủ/không đủ và chi tiết nếu đủ
        /// </summary>
        [HttpGet("CompareBlood/{emergencyId}")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> CompareBlood(int emergencyId)
        {
            try
            {
                var result = await _emergencyService.CompareBloodForEmergency(emergencyId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error comparing blood for emergency");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Sửa thông tin đơn khẩn cấp (chỉ người tạo hoặc Admin/Staff)
        /// </summary>
        [HttpPut("UpdateEmergency/{emergencyId}")]
        [Authorize(Roles = "User,Staff,Admin")]
        public async Task<IActionResult> UpdateEmergency(int emergencyId, [FromBody] RegisterEmergencyDto dto)
        {
            try
            {
                var username = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
                var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
                if (username == null || role == null)
                    return Unauthorized(new { message = "User not authenticated." });

                var result = await _emergencyService.UpdateEmergency(emergencyId, username, role, dto);
                if (result == "Emergency updated successfully.")
                    return Ok(new { message = result });
                return BadRequest(new { message = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating emergency");
                return StatusCode(500, new { message = "An error occurred while processing your request.", error = ex.Message });
            }
        }

        /// <summary>
        /// Xóa đơn khẩn cấp (chỉ người tạo hoặc Admin/Staff)
        /// </summary>
        [HttpDelete("DeleteEmergency/{emergencyId}")]
        [Authorize(Roles = "User,Staff,Admin")]
        public async Task<IActionResult> DeleteEmergency(int emergencyId)
        {
            try
            {
                var username = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
                var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
                if (username == null || role == null)
                    return Unauthorized(new { message = "User not authenticated." });

                var result = await _emergencyService.DeleteEmergency(emergencyId, username, role);
                if (result == "Emergency deleted successfully.")
                    return Ok(new { message = result });
                return BadRequest(new { message = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting emergency");
                return StatusCode(500, new { message = "An error occurred while processing your request.", error = ex.Message });
            }
        }
    }
} 