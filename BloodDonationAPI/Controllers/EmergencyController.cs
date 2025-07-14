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
        private readonly INotificationService _notificationService;
        private readonly ILogger<EmergencyController> _logger;

        public EmergencyController(IEmergencyService emergencyService, INotificationService notificationService, ILogger<EmergencyController> logger)
        {
            _emergencyService = emergencyService;
            _notificationService = notificationService;
            _logger = logger;
        }
        /// <summary>
        /// Dùng để đăng ký đơn khẩn cấp cần đăng nhập 
        /// </summary>
        
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("RegisterEmergency")]
        [Authorize(Roles = "User,Staff,Admin")]
        public async Task<IActionResult> RegisterEmergency([FromBody] RegisterEmergencyDto dto)
        {
            try
            {
                var username = User.FindFirst(ClaimTypes.Name)?.Value;
                if (username == null)
                    return Unauthorized(new { message = "User not authenticated." });

                var role = User.FindFirst(ClaimTypes.Role)?.Value;
                if (role == null)
                    return Unauthorized(new { message = "User role not found." });

                if (string.IsNullOrEmpty(dto.BloodType))
                    return BadRequest(new { message = "Blood type is required." });

                var validBloodTypes = new[] { "A+", "A-", "B+", "B-", "O+", "O-", "AB+", "AB-" };
                var normalizedBloodType = dto.BloodType.Trim().ToUpper();
                if (!validBloodTypes.Contains(normalizedBloodType))
                    return BadRequest(new { message = "BloodType must be one of: A+, A-, B+, B-, O+, O-, AB+, AB-." });

                if (!dto.RequiredUnits.HasValue || dto.RequiredUnits <= 0)
                    return BadRequest(new { message = "RequiredUnits must be greater than 0." });

                if (dto.EndDate.HasValue && dto.EndDate < DateOnly.FromDateTime(DateTime.Now))
                    return BadRequest(new { message = "EndDate cannot be in the past." });

                var result = await _emergencyService.RegisterEmergency(username, role, dto);
                if (result.IsSuccess)
                {
                    // Nếu là Staff hoặc Admin, tự động tạo notification
                    if (role == "Staff" || role == "Admin")
                    {
                        try
                        {
                            var notificationResult = await _notificationService.CreateNotificationForEmergency(result.EmergencyId.Value);
                            if (notificationResult == "Notification created successfully.")
                            {
                                return Ok(new { 
                                    message = result.Message, 
                                    emergencyId = result.EmergencyId,
                                    notificationMessage = "Notification created automatically for staff/admin emergency." 
                                });
                            }
                            else
                            {
                                _logger.LogWarning("Failed to create notification automatically: {NotificationResult}", notificationResult);
                                return Ok(new { 
                                    message = result.Message, 
                                    emergencyId = result.EmergencyId,
                                    notificationMessage = "Emergency created but notification creation failed: " + notificationResult 
                                });
                            }
                        }
                        catch (Exception notificationEx)
                        {
                            _logger.LogError(notificationEx, "Error creating notification automatically for staff/admin emergency");
                            return Ok(new { 
                                message = result.Message, 
                                emergencyId = result.EmergencyId,
                                notificationMessage = "Emergency created but notification creation failed due to an error." 
                            });
                        }
                    }
                    // Nếu là User, tạo notification cho staff/admin biết có đơn mới
                    else if (role == "User")
                    {
                        try
                        {
                            var adminNotificationResult = await _notificationService.CreateAdminNotificationForNewEmergency(result.EmergencyId.Value, username);
                            // Không cần kiểm tra kết quả, chỉ log nếu lỗi
                            if (adminNotificationResult != "Emergency notification for new emergency created successfully.")
                            {
                                _logger.LogWarning("Failed to create Emergency notification: {Result}", adminNotificationResult);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error creating Emergency notification for new emergency");
                        }
                    }
                    return Ok(new { 
                        message = result.Message, 
                        emergencyId = result.EmergencyId 
                    });
                }

                return BadRequest(new { message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering emergency");
                return StatusCode(500, new { message = "An error occurred while processing your request.", error = ex.Message });
            }
        }
        
        /// <summary>
        /// Dùng để hiển thị tất cả danh sách các đơn khẩn cấp chỉ dành cho staff và admin
        /// </summary>
        ///<remarks>
        /// Nếu staff và admin tạo, thông báo sẽ được tự động tạo
        ///</remarks>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpGet("GetEmergencies")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> GetEmergencies()
        {
            try
            {
                var emergencies = await _emergencyService.GetEmergencies();
                var result = emergencies.Select(e => new {
                    e.EmergencyId,
                    e.Username,
                    e.EmergencyDate,
                    e.BloodType,
                    e.EmergencyStatus,
                    e.EmergencyNote,
                    e.RequiredUnits,
                    e.HospitalId,
                    e.EmergencyMedical,
                    e.EmergencyImage,
                    e.EndDate
                });
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting emergencies");
                return StatusCode(500, new { message = "An error occurred while processing your request.", error = ex.Message });
            }
        }

        /// <summary>
        /// Dùng để xét duyệt đơn đăng ký khẩn cấp, nếu admin or staff tạo đơn thì mặc định đã xét duyệt
        /// </summary>
        /// <remarks>
        /// Cần nhập đúng chuỗi "Đã xét duyệt" or "Từ chối"
        /// Sau khi xác nhận "Đã xét duyệt" thông báo sẽ tự động tạo
        /// </remarks>
        /// <param name="dto"></param>
        /// <returns></returns>
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
                {
                    // Nếu status được cập nhật thành "Đã xét duyệt", tự động tạo notification
                    if (status == "Đã xét duyệt")
                    {
                        try
                        {
                            var notificationResult = await _notificationService.CreateNotificationForEmergency(emergencyId);
                            if (notificationResult == "Notification created successfully.")
                            {
                                return Ok(new { 
                                    message = result, 
                                    notificationMessage = "Notification created automatically for approved emergency." 
                                });
                            }
                            else
                            {
                                _logger.LogWarning("Failed to create notification automatically: {NotificationResult}", notificationResult);
                                return Ok(new { 
                                    message = result, 
                                    notificationMessage = "Emergency status updated but notification creation failed: " + notificationResult 
                                });
                            }
                        }
                        catch (Exception notificationEx)
                        {
                            _logger.LogError(notificationEx, "Error creating notification automatically for approved emergency");
                            return Ok(new { 
                                message = result, 
                                notificationMessage = "Emergency status updated but notification creation failed due to an error." 
                            });
                        }
                    }
                    
                    return Ok(new { message = result });
                }

                return BadRequest(new { message = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating emergency status");
                return StatusCode(500, new { message = "An error occurred while processing your request.", error = ex.Message });
            }
        }

        /// <summary>
        /// So sánh lượng máu trong kho với đơn khẩn cấp, trả về trạng thái đủ/không đủ và chi tiết nếu đủ
        /// </summary>
        [HttpGet("CompareBlood/{emergencyId}")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> CompareBlood(int emergencyId)
        {
            try
            {
                var result = await _emergencyService.CompareBloodForEmergency(emergencyId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error comparing blood for emergency");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Sửa thông tin đơn khẩn cấp (chỉ người tạo hoặc Admin/Staff)
        /// </summary>
        [HttpPut("UpdateEmergency/{emergencyId}")]
        [Authorize(Roles = "User,Staff,Admin")]
        public async Task<IActionResult> UpdateEmergency(int emergencyId, [FromBody] RegisterEmergencyDto dto)
        {
            try
            {
                var username = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
                var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
                if (username == null || role == null)
                    return Unauthorized(new { message = "User not authenticated." });

                if (dto.EndDate.HasValue && dto.EndDate < DateOnly.FromDateTime(DateTime.Now))
                    return BadRequest(new { message = "EndDate cannot be in the past." });

                var result = await _emergencyService.UpdateEmergency(emergencyId, username, role, dto);
                if (result == "Emergency updated successfully.")
                    return Ok(new { message = result });
                return BadRequest(new { message = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating emergency");
                return StatusCode(500, new { message = "An error occurred while processing your request.", error = ex.Message });
            }
        }

        /// <summary>
        /// Xóa đơn khẩn cấp (chỉ người tạo hoặc Admin/Staff)
        /// </summary>
        [HttpDelete("DeleteEmergency/{emergencyId}")]
        [Authorize(Roles = "User,Staff,Admin")]
        public async Task<IActionResult> DeleteEmergency(int emergencyId)
        {
            try
            {
                var username = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
                var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
                if (username == null || role == null)
                    return Unauthorized(new { message = "User not authenticated." });

                var result = await _emergencyService.DeleteEmergency(emergencyId, username, role);
                if (result == "Emergency deleted successfully.")
                    return Ok(new { message = result });
                return BadRequest(new { message = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting emergency");
                return StatusCode(500, new { message = "An error occurred while processing your request.", error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy danh sách các đơn khẩn cấp do chính user tạo
        /// </summary>
        [HttpGet("GetMyEmergencies")]
        [Authorize(Roles = "User,Staff,Admin")]
        public async Task<IActionResult> GetMyEmergencies()
        {
            try
            {
                var username = User.FindFirst(ClaimTypes.Name)?.Value;
                if (string.IsNullOrEmpty(username))
                    return Unauthorized(new { message = "User not authenticated." });

                var emergencies = await _emergencyService.GetEmergenciesByUsername(username);
                var result = emergencies.Select(e => new {
                    e.EmergencyId,
                    e.Username,
                    e.EmergencyDate,
                    e.BloodType,
                    e.EmergencyStatus,
                    e.EmergencyNote,
                    e.RequiredUnits,
                    e.HospitalId,
                    e.EmergencyMedical,
                    e.EmergencyImage,
                    e.EndDate
                });
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting my emergencies");
                return StatusCode(500, new { message = "An error occurred while processing your request.", error = ex.Message });
            }
        }

        /// <summary>
        /// Cho phép người tạo đơn cập nhật trạng thái emergencyStatus thành 'Đã được đáp ứng' nếu trạng thái cũ là 'Đã xét duyệt' và 
        /// </summary>
        [HttpPut("MarkAsFulfilled/{emergencyId}")]
        [Authorize(Roles = "User,Staff,Admin")]
        public async Task<IActionResult> MarkAsFulfilled(int emergencyId)
        {
            try
            {
                var username = User.FindFirst(ClaimTypes.Name)?.Value;
                if (string.IsNullOrEmpty(username))
                    return Unauthorized(new { message = "User not authenticated." });

                var result = await _emergencyService.MarkEmergencyAsFulfilled(emergencyId, username);
                if (result == "Emergency marked as fulfilled.")
                    return Ok(new { message = result });
                return BadRequest(new { message = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking emergency as fulfilled");
                return StatusCode(500, new { message = "An error occurred while processing your request.", error = ex.Message });
            }
        }

        /// <summary>
        /// Đặt trạng thái đơn khẩn cấp thành 'Lượng máu đang được chuyển đến' (chỉ cần emergencyId)
        /// </summary>
        [HttpPut("SetStatusTransferring/{emergencyId}")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> SetStatusTransferring(int emergencyId)
        {
            var result = await _emergencyService.SetEmergencyStatusToTransferring(emergencyId);
            if (result.Contains("set to"))
                return Ok(new { message = result });
            return BadRequest(new { message = result });
        }
    }
} 