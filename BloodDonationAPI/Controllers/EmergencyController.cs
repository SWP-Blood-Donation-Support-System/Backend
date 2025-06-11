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

        [HttpPost("RegisterEmergency")]
        [Authorize(Roles = "User,Staff,Admin")]
        public async Task<IActionResult> RegisterEmergency([FromBody] RegisterEmergencyDto dto)
        {
            try
            {
                var username = User.FindFirst(ClaimTypes.Name)?.Value;
                if (username == null)
                    return Unauthorized(new { message = "User not authenticated." });

                if (string.IsNullOrEmpty(dto.BloodType))
                    return BadRequest(new { message = "Blood type is required." });

                var result = await _emergencyService.RegisterEmergency(username, dto);
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