using BloodDonationAPI.DTO;
using BloodDonationAPI.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BloodDonationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentServiece  _appointmentService;
            public AppointmentController(IAppointmentServiece appointmentService)
        {
            _appointmentService = appointmentService;
        }

        [HttpGet("GetAppointmentLists")]
        public async Task<IActionResult> GetAppointmentLists()
        {
           var appointmentLists = await _appointmentService.GetAppointmentLists();
            if (appointmentLists == null || !appointmentLists.Any())
            {
                return NotFound(new { message = "No appointments found." });
            }
            return Ok(appointmentLists);
        }

        [HttpPost("RegisterAppointment")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> RegisterAppointment([FromBody] RegisterAppointmentDto dto)
        {
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            if(userName == null) return Unauthorized(new { message = "User not authenticated." });
            var result = await _appointmentService.RegisterAppointment(userName, dto);
            if (result == "Success") return Ok(result);

            else return BadRequest(new { message = result });
        }

        [HttpGet("history/{username}")]
        public async Task<IActionResult> GetAppointmentHistoryByUsername(string username)
        {
            var histories = await _appointmentService.GetByUsernameAsync(username);

            if (histories == null || !histories.Any())
            {
                return NotFound(new { message = "No appointment history found for this user." });
            }

            return Ok(histories);
        }
        [HttpPut("CancelAppointment/{id}")]
        public async Task<IActionResult> CancelAppointment(int id)
        {
            var result = await _appointmentService.CancelAppointmentAsync(id);

            if (!result)
                return NotFound(new { message = "Lịch hẹn không tồn tại hoặc đã bị hủy." });

            return Ok(new { message = "Đã hủy lịch hẹn thành công." });
        }
    }
}
