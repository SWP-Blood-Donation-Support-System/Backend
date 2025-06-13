using BloodDonationAPI.DTO;
using BloodDonationAPI.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BloodDonationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(INotificationService notificationService, ILogger<NotificationController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        /// <summary>
        /// Dùng để lấy tất cả các thông báo được gửi đi
        /// </summary>
        
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpGet("GetNotifications")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> GetNotifications()
        {
            try
            {
                var notifications = await _notificationService.GetNotifications();
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notifications");
                return StatusCode(500, new { message = "An error occurred while processing your request.", error = ex.Message });
            }
        }

        /// <summary>
        /// Dùng để tạo thông báo bằng cách nhập id đơn khẩn cấp đã được xét duyệt
        /// </summary>
        /// <remarks>
        /// Hệ thống sẽ tự động gửi thông báo đến các user có cùng nhóm máu với đơn khẩn cấp
        /// </remarks>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("CreateNotification/{emergencyId}")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> CreateNotification(int emergencyId)
        {
            try
            {
                var result = await _notificationService.CreateNotificationForEmergency(emergencyId);
                if (result == "Notification created successfully.")
                    return Ok(new { message = result });

                return BadRequest(new { message = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification");
                return StatusCode(500, new { message = "An error occurred while processing your request.", error = ex.Message });
            }
        }

        /// <summary>
        /// Dùng để tìm thông báo theo nhóm máu
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpGet("GetNotificationsByBloodType/{bloodType}")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> GetNotificationsByBloodType(string bloodType)
        {
            try
            {
                var notifications = await _notificationService.GetNotificationsByBloodType(bloodType);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notifications by blood type");
                return StatusCode(500, new { message = "An error occurred while processing your request.", error = ex.Message });
            }
        }
        
        /// <summary>
        /// Dùng để user lấy thông báo được gửi đến
        /// </summary>
        /// <remarks>
        /// Chỉ xem được thông báo ngưới đến tk đăng nhập
        /// </remarks>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpGet("GetMyNotifications")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetMyNotifications()
        {
            try
            {
                var username = User.FindFirst(ClaimTypes.Name)?.Value;
                if (username == null)
                    return Unauthorized(new { message = "User not authenticated." });

                var notifications = await _notificationService.GetUserNotifications(username);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user notifications");
                return StatusCode(500, new { message = "An error occurred while processing your request.", error = ex.Message });
            }
        }

        /// <summary>
        /// Dùng để phản hồi thông báo của người dùng
        /// </summary>
        /// <remarks>
        /// Cần nhập đúng chuỗi "Chấp nhật" or "Từ chối"
        /// </remarks>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPut("UpdateResponse/{notificationId}")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> UpdateNotificationResponse(int notificationId, [FromBody] string responseStatus)
        {
            try
            {
                var username = User.FindFirst(ClaimTypes.Name)?.Value;
                if (username == null)
                    return Unauthorized(new { message = "User not authenticated." });

                var result = await _notificationService.UpdateNotificationResponse(notificationId, username, responseStatus);
                if (result == "Notification response updated successfully.")
                    return Ok(new { message = result });

                return BadRequest(new { message = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating notification response");
                return StatusCode(500, new { message = "An error occurred while processing your request.", error = ex.Message });
            }
        }
    }
} 