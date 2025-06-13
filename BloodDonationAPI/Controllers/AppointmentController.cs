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
        /// <summary>
        /// Lấy tất cả danh sách các event hiến máu mà chưa tới ngày hiến máu
        /// </summary>
        /// <remarks>
        /// Api này sẻ trả về danh sách các lịch hẹn hiến máu mà chưa tới ngày hiến máu,
        /// </remarks>
        /// <returns></returns>
        [HttpGet("GetEventsLists")]
        public async Task<IActionResult> GetAppointmentLists()
        {
           var appointmentLists = await _appointmentService.GetEventsLists();
            if (appointmentLists == null || !appointmentLists.Any())
            {
                return NotFound(new { message = "No appointments found." });
            }
            return Ok(appointmentLists);
        }
        /// <summary>
        /// Dùng để đăng ký lịch hẹn hiến máu nhập vào eventID và phải là roll user mới được phép đăng ký 
        /// </summary>
        /// <remarks>
        /// API này chạy bằng cách nhập eventID muốn đăng ký vào và lấy username đăng nhập để thêm thông tin vào bảng lưu trữ thông tin 
        /// 
        /// (front end sẻ hiển thị mấy hết event ra và có nút dăng ký nó sẻ lấy eventID của cái lịch mà mình bấm vào và gửi vào đây)
        /// </remarks>
        /// <param name="dto"></param>
        /// <returns></returns>
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
        /// <summary>
        /// dùng để xem xem user nào đã dăng ký những lịch hẹn nào 
        /// </summary>
        /// <remarks>
        /// API này dùng bằng cách nhập userName vào và sẻ hiển thị tất cả các lịch hẹn đã đăng ký và có thể xem chi tiết lịch hẹn đó 
        /// 
        /// FE thì sẻ lấy dữ liệu là usernào đang login thì sẻ cho xem các lịch hẹn đã đăng kí của user đó
        /// </remarks>
        /// <param name="username"></param>
        /// <returns></returns>
        [HttpGet("AppointmentHistory/{username}")]
        public async Task<IActionResult> GetAppointmentHistoryByUsername(string username)
        {
            var histories = await _appointmentService.GetByUsernameAsync(username);

            if (histories == null || !histories.Any())
            {
                return NotFound(new { message = "No appointment history found for this user." });
            }

            return Ok(histories);
        }
        /// <summary>
        /// dùng để cập nhật trạng thái trong lịch hẹn thành đã hủy
        /// </summary>
        /// <remarks>
        /// API dùng bằng cách nhập RecordID của cái bảng lưu thông tin dăng ký lịch hẹn sau khi nhập vào sẻ sữa status thành Canceled 
        /// 
        /// FE thì khi cho xem các lịch hẹn đã đăng ký thì sẽ có nút hủy lịch hẹn, khi bấm vào nó sẻ lấy RecordID của cái lịch hẹn đó và gửi vào đây và chỉnh sửa trạng thái thành Canceled
        /// </remarks>
        /// <param name="id"></param>
        /// <returns></returns>
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
