using BloodDonationAPI.DTO;
using BloodDonationAPI.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BloodDonationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BloodDonationProcessController : ControllerBase
    {
        private readonly IBloodDonationProcessService _service;

        public BloodDonationProcessController(IBloodDonationProcessService service)
        {
            _service = service;
        }


        [HttpGet("GetRegistrationsByAppointmentID/{AppointmentID}")]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> GetRegistrationsByAppointmentID(int AppointmentID)
        {
            var result = await _service.GetRegistrationsByAppointmentID(AppointmentID);
            if (result == null || !result.Any())
            {
                return NotFound("No registrations found for this appointment ID.");
            }
            return Ok(result);
        }

        [HttpPut("UpdateAppointmentStatus")]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> UpdateAppointmentStatus([FromBody] UpdateAppointmentStatusDto updateDto)
        {
            if (updateDto == null || updateDto.AppointmentHistoryId <= 0)
            {
                return BadRequest("Invalid appointment history ID.");
            }

            var result = await _service.UpdateAppointmentStatusAsync(updateDto);
            if (!result)
            {
                return NotFound("Appointment history not found or update failed.");
            }
            return Ok("Appointment status updated successfully.");
        }
    }

}
