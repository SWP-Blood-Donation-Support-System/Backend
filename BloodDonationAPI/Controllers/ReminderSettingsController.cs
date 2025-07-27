using BloodDonationAPI.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BloodDonationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReminderSettingsController : ControllerBase
    {
        private readonly ReminderSettings _reminderSettings;
        public ReminderSettingsController(ReminderSettings reminderSettings)
        {
            _reminderSettings = reminderSettings;
        }
        [HttpGet]
        public IActionResult GetInterval()
        {
            return Ok(new { IntervalInMinutes = _reminderSettings.ReminderInterval.TotalMinutes });
        }
        [HttpPost]
        public IActionResult SetInterval([FromBody] int minutes)
        {
            if(minutes <= 0 || minutes >1440)
            {
                return BadRequest("Interval must be between 1 and 1440 minutes (1 day).");
            }
            _reminderSettings.ReminderInterval = TimeSpan.FromMinutes(minutes);
            return Ok(new { Message = $"Reminder interval set to {minutes} minutes." });
        }
    }
}
