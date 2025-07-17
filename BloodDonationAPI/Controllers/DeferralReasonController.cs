using BloodDonationAPI.DTO;
using BloodDonationAPI.Service;
using Microsoft.AspNetCore.Http;
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
    }
}
