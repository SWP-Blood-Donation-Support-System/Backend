using BloodDonationAPI.DTO;
using BloodDonationAPI.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace BloodDonationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeferralReasonController : ControllerBase
    {
        private readonly IDeferralReasonService _deferralReasonService;

        public DeferralReasonController(IDeferralReasonService deferralReasonService)
        {
            _deferralReasonService = deferralReasonService;
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllDeferralReasons()
        {
            var deferralReasons = await _deferralReasonService.GetAllDeferralReasonsAsync();
            if (deferralReasons == null || !deferralReasons.Any())
            {
                return NotFound(new { message = "No deferral reasons found." });
            }
            return Ok(deferralReasons);
        }
        [HttpPost("create")]
        public async Task<IActionResult> CreateDeferralReason([FromBody] DeferralReasonDto createDeferralReasonDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Dữ liệu không hợp lệ", errors = ModelState });

            var result = await _deferralReasonService.AddDeferralReasonAsync(createDeferralReasonDto);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message });
        }


        [HttpPut("update")]
        public async Task<IActionResult> UpdateDeferralReason([FromBody] UpdateDeferralReasonDto updateDeferralReasonDto)
        {
            if (updateDeferralReasonDto == null)
            {
                return BadRequest( new { message = "Invalid deferral reason data." });
            }
            var result = await _deferralReasonService.UpdateDeferralReasonAsync(updateDeferralReasonDto);
            if (!result)
            {
                return NotFound(new { message = "Deferral reason not found." });
            }
            return Ok(new { message = "Deferral reason updated successfully." });
        }

        [HttpDelete("delete/{reasonCode}")]
        public async Task<IActionResult> DeleteDeferralReason(string reasonCode)
        {
            if (string.IsNullOrEmpty(reasonCode))
            {
                return BadRequest(new { message = "Invalid reason code." });
            }
            var result = await _deferralReasonService.DeleteDeferralReasonAsync(reasonCode);
            if (!result)
            {
                return NotFound(new { message = "Deferral reason not found." });
            }
            return Ok(new { message = "Deferral reason deleted successfully." });
        }
    }
}
