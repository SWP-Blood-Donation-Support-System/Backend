using BloodDonationAPI.Entities;
using BloodDonationAPI.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BloodDonationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly IEventService _eventService;
        public EventController(IEventService eventService)
        {
            _eventService = eventService;
        }

        /// <summary>
        /// dung de lay tat ca cac su kien
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetAllEvents")]
        public async Task<IActionResult> GetAllEvents()
        {
            var events = await _eventService.GetAllEventsAsync();
            return Ok(events);
        }
        /// <summary>
        /// lay su kien theo id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("getEventById/{id}")]
        public async Task<IActionResult> GetEventById(int id)
        {
            var eventItem = await _eventService.GetEventByIdAsync(id);
            if (eventItem == null)
            {
                return NotFound();
            }
            return Ok(eventItem);
        }

        /// <summary>
        /// tao moi su kien 
        /// </summary>
        /// <remarks>
        /// đừng coi cái json nó hiển thị cái đó không chạy được đâu 
        /// 
        /// {
        ///  "eventDate": "2025-07-01",
        ///  
        ///  "eventTime": "09:30:00",
        ///  
        ///  "eventTitle": "Sự kiện hiến máu mùa hè",
        ///  
        ///  "eventContent": "Tham gia hiến máu để cứu người",
        ///  
        ///  "location": "Trung tâm y tế quận 1",
        ///  
        ///  "maxParticipants": 50
        ///}
        ///cái này là cái json ví dụ chạy được 
        /// </remarks>
        /// <param name="newEvent"></param>
        /// <returns></returns>
        [HttpPost("createEvent")]
        public async Task<IActionResult> CreateEvent([FromBody] Event newEvent)
        {
            if (newEvent == null)
            {
                return BadRequest("Invalid event data.");
            }
            await _eventService.AddEventAsync(newEvent);
            return CreatedAtAction(nameof(GetEventById), new { id = newEvent.EventId }, newEvent);
        }
        /// <summary>
        /// dung de cap nhat event theo id
        /// </summary>
        /// <remarks>
        /// json nó hiển thị cũng không chạy được 
        /// 
        /// {
        ///  "eventDate": "2025-07-15",
        ///  
        /// "eventTime": "14:00:00",
        /// 
        ///  "eventTitle": "Sự kiện hiến máu mùa hè (cập nhật)",
        ///  
        ///  "eventContent": "Tham gia hiến máu cứu người - cập nhật thông tin",
        ///  
        ///  "location": "Trung tâm y tế quận 1",
        ///  
        ///  "maxParticipants": 100
        ///}
        ///day la json ví dụ chạy được
        /// 
        /// </remarks>
        /// <param name="id"></param>
        /// <param name="updatedEvent"></param>
        /// <returns></returns>

        [HttpPut("UpdateEvent/{id}")]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] Event updatedEvent)
        {
            if (updatedEvent == null )
            {
                return BadRequest("Invalid event data.");
            }
            await _eventService.UpdateEventAsync(id, updatedEvent);
            return Ok(new { message = "da cap nhat thanh cong" });
        }

        /// <summary>
        /// nay de xoa su kien theo id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("DeleteEvent/{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            await _eventService.DeleteEventAsync(id);
            return NoContent();
        }
    }
}
