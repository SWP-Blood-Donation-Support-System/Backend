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


        [HttpGet("GetRegisterListByEventID/{AppointmentID}")]
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
        [HttpPost("AddDonationHistory")]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> AddDonationHistory([FromBody] CreateDonationHistoryDto registrationDto)
        {
            if (registrationDto == null || string.IsNullOrEmpty(registrationDto.Username))
            {
                return BadRequest("Invalid donation history data.");
            }
            var result = await _service.AddDonationHistoryAsync(registrationDto);
            if (!result)
            {
                return NotFound("User not found or donation history could not be added.");
            }
            return Ok("Donation history added successfully.");
        }
        [HttpPost("AddBloodToBank")]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> AddBloodToBank([FromBody] AddBloodBankDto dto)
        {
            try
            {
                var result = await _service.AddBloodToBankAsync(dto);
                if (result == null)
                {
                    return BadRequest("Failed to add blood to the bank. Please check the input data.");
                }
                else
                {
                    return Ok(new
                    {
                        message = "Blood added to bank successfully.",
                        bloodBankId = result.BloodTypeId,
                        bloodType = result.BloodTypeName,
                        unit = result.Unit,
                        expiryDate = result.ExpiryDate
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while adding blood to the bank", error = ex.Message });
            }
           
        }

        [HttpGet("GetDonationHistoryByUserName/{username}")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetDonationHistoryByUserName(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return BadRequest("Username cannot be null or empty.");
            }
            var result = await _service.GetDonationHistoryByUserNameAsync(username);
            if (result == null || !result.Any())
            {
                return NotFound("No donation history found for this user.");
            }
            return Ok(result);
        }
    }

}
