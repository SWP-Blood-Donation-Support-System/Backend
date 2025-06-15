using BloodDonationAPI.DTO;
using BloodDonationAPI.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BloodDonationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly ILogger<ReportController> _logger;

        public ReportController(IReportService reportService, ILogger<ReportController> logger)
        {
            _reportService = reportService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy tất cả các báo cáo (chỉ dành cho Admin và Staff)
        /// </summary>
        [HttpGet("GetAllReports")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetAllReports()
        {
            try
            {
                var reports = await _reportService.GetAllReports();
                return Ok(reports);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all reports");
                return StatusCode(500, new { message = "An error occurred while processing your request.", error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy báo cáo theo ID
        /// </summary>
        [HttpGet("GetReportById/{reportId}")]
        [Authorize]
        public async Task<IActionResult> GetReportById(int reportId)
        {
            try
            {
                var report = await _reportService.GetReportById(reportId);
                if (report == null)
                    return NotFound(new { message = "Report not found." });

                var username = User.FindFirst(ClaimTypes.Name)?.Value;
                var role = User.FindFirst(ClaimTypes.Role)?.Value;

                if (report.Username != username && role != "Admin" && role != "Staff")
                    return Forbid();

                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting report by id");
                return StatusCode(500, new { message = "An error occurred while processing your request.", error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy tất cả báo cáo của người dùng đang đăng nhập
        /// </summary>
        [HttpGet("GetMyReports")]
        [Authorize]
        public async Task<IActionResult> GetMyReports()
        {
            try
            {
                var username = User.FindFirst(ClaimTypes.Name)?.Value;
                if (username == null)
                    return Unauthorized(new { message = "User not authenticated." });

                var reports = await _reportService.GetUserReports(username);
                return Ok(reports);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user reports");
                return StatusCode(500, new { message = "An error occurred while processing your request.", error = ex.Message });
            }
        }

        /// <summary>
        /// Tạo báo cáo mới
        /// </summary>
        [HttpPost("CreateReport")]
        [Authorize]
        public async Task<IActionResult> CreateReport([FromBody] CreateReportDto dto)
        {
            try
            {
                var username = User.FindFirst(ClaimTypes.Name)?.Value;
                if (username == null)
                    return Unauthorized(new { message = "User not authenticated." });

                var result = await _reportService.CreateReport(username, dto);
                if (result == "Report created successfully.")
                    return Ok(new { message = result });

                return BadRequest(new { message = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating report");
                return StatusCode(500, new { message = "An error occurred while processing your request.", error = ex.Message });
            }
        }

        /// <summary>
        /// Cập nhật báo cáo
        /// </summary>
        [HttpPut("UpdateReport/{reportId}")]
        [Authorize]
        public async Task<IActionResult> UpdateReport(int reportId, [FromBody] UpdateReportDto dto)
        {
            try
            {
                var username = User.FindFirst(ClaimTypes.Name)?.Value;
                if (username == null)
                    return Unauthorized(new { message = "User not authenticated." });

                var result = await _reportService.UpdateReport(reportId, username, dto);
                if (result == "Report updated successfully.")
                    return Ok(new { message = result });

                return BadRequest(new { message = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating report");
                return StatusCode(500, new { message = "An error occurred while processing your request.", error = ex.Message });
            }
        }

        /// <summary>
        /// Xóa báo cáo
        /// </summary>
        [HttpDelete("DeleteReport/{reportId}")]
        [Authorize]
        public async Task<IActionResult> DeleteReport(int reportId)
        {
            try
            {
                var username = User.FindFirst(ClaimTypes.Name)?.Value;
                if (username == null)
                    return Unauthorized(new { message = "User not authenticated." });

                var result = await _reportService.DeleteReport(reportId, username);
                if (result == "Report deleted successfully.")
                    return Ok(new { message = result });

                return BadRequest(new { message = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting report");
                return StatusCode(500, new { message = "An error occurred while processing your request.", error = ex.Message });
            }
        }
    }
} 