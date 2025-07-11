using BloodDonationAPI.Entities;
using BloodDonationAPI.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BloodDonationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _searchService;

        public SearchController(ISearchService searchService)
        {
            _searchService = searchService;
        }        /// <summary>
        /// Tìm kiếm người hiến máu theo nhóm máu
        /// </summary>
        /// <remarks>
        /// API này cho phép tìm kiếm người hiến máu theo nhóm máu cụ thể.
        /// API này có thể truy cập mà không cần xác thực để hỗ trợ test.
        /// </remarks>
        /// <param name="bloodType">Nhóm máu (A+, A-, B+, B-, AB+, AB-, O+, O-)</param>
        /// <returns>Danh sách người hiến máu có nhóm máu tương thích, sắp xếp theo địa chỉ</returns>
        [HttpGet("donors/byBloodType")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> GetDonorsByBloodType([FromQuery] string bloodType)
        {
            try
            {                // Lấy thông tin người dùng hiện tại từ token
                var currentUser = User.Identity?.Name ?? string.Empty;
                var role = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value ?? string.Empty;


                // Validate input parameters
                if (string.IsNullOrWhiteSpace(bloodType))
                {
                    return BadRequest(new { message = "BloodType is required" });
                }

                // Chuẩn hóa bloodType
                var normalizedBloodType = bloodType.ToUpper().Trim();
                var validBloodTypes = new[] { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" };
                
                if (!validBloodTypes.Contains(normalizedBloodType))
                {
                    return BadRequest(new { message = "Invalid blood type. Must be A+, A-, B+, B-, AB+, AB-, O+, or O-" });
                }
                
                // Gọi service để tìm kiếm người hiến máu
                var donors = await _searchService.FindDonorsByBloodType(bloodType);
                
                // Log số lượng để kiểm tra
                Console.WriteLine($"Found {donors.Count()} donors for blood type {bloodType}");
                
                return Ok(new { 
                    donors,
                    message = "Donor search successful. Results are sorted by distance from reference point (7 Đ. D1, Long Thạnh Mỹ, Thủ Đức)",
                    currentUser = currentUser,
                    role = role,
                    searchCriteria = new {
                        bloodType = normalizedBloodType,
                        referencePoint = "7 Đ. D1, Long Thạnh Mỹ, Thủ Đức, Hồ Chí Minh"
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetDonorsByBloodType: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }        /// <summary>
        /// Tìm kiếm yêu cầu cần máu theo nhóm máu
        /// </summary>
        /// <remarks>
        /// API này cho phép tìm kiếm các yêu cầu cần máu theo nhóm máu cụ thể.
        /// Chỉ Admin, Staff, và User mới có thể sử dụng chức năng này.
        /// Kết quả được sắp xếp theo khoảng cách từ điểm mốc: 7 Đ. D1, Long Thạnh Mỹ, Thủ Đức.
        /// Chỉ hiển thị các yêu cầu có trạng thái "Đã xét duyệt".
        /// </remarks>
        /// <param name="bloodType">Nhóm máu (A+, A-, B+, B-, AB+, AB-, O+, O-)</param>
        /// <returns>Danh sách người cần máu có nhóm máu đã chỉ định, sắp xếp theo khoảng cách</returns>
        [HttpGet("requests/byBloodType")]
        [Authorize(Roles = "Admin,Staff,User")]
        public async Task<IActionResult> GetBloodRequestsByBloodType([FromQuery] string bloodType)
        {
            try
            {
                // Lấy thông tin người dùng hiện tại từ token
                var currentUser = User.Identity?.Name ?? string.Empty;
                var role = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value ?? string.Empty;

                // Validate input parameters
                if (string.IsNullOrWhiteSpace(bloodType))
                {
                    return BadRequest(new { message = "BloodType is required" });
                }

                // Chuẩn hóa bloodType
                var normalizedBloodType = bloodType.ToUpper().Trim();
                var validBloodTypes = new[] { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" };
                
                if (!validBloodTypes.Contains(normalizedBloodType))
                {
                    return BadRequest(new { message = "Invalid blood type. Must be A+, A-, B+, B-, AB+, AB-, O+, or O-" });
                }
                
                // Gọi service để tìm kiếm yêu cầu máu
                var bloodRequests = await _searchService.FindBloodRequestsByBloodType(normalizedBloodType);
                
                // Log số lượng để kiểm tra
                Console.WriteLine($"Found {bloodRequests.Count()} approved blood requests for blood type {normalizedBloodType}");
                
                return Ok(new { 
                    bloodRequests,
                    message = "Blood request search successful. Results are sorted by distance from reference point and only show approved requests.",
                    currentUser = currentUser,
                    role = role,
                    searchCriteria = new {
                        bloodType = normalizedBloodType,
                        statusFilter = "Đã xét duyệt",
                        referencePoint = "7 Đ. D1, Long Thạnh Mỹ, Thủ Đức, Hồ Chí Minh"
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetBloodRequestsByBloodType: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Tìm kiếm tất cả người hiến máu có điều kiện
        /// </summary>
        /// <remarks>
        /// API này trả về danh sách tất cả người hiến máu đã hoàn thành hiến máu và có trạng thái Active.
        /// Điều kiện lọc:
        /// - User.ProfileStatus = "Active"
        /// - Có ít nhất 1 AppointmentRecord với Status = "Đã hiến"
        /// - Kết quả được sắp xếp theo khoảng cách từ điểm mốc: 7 Đ. D1, Long Thạnh Mỹ, Thủ Đức
        /// </remarks>
        /// <returns>Danh sách tất cả người hiến máu có điều kiện, sắp xếp theo khoảng cách</returns>
        [HttpGet("donors/all")]
        [Authorize(Roles = "Admin,Staff,User")]
        public async Task<IActionResult> GetAllAvailableDonors()
        {
            try
            {
                // Lấy thông tin người dùng hiện tại từ token
                var currentUser = User.Identity?.Name ?? string.Empty;
                var role = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value ?? string.Empty;

                // Gọi service để lấy tất cả người hiến máu có điều kiện
                var donors = await _searchService.FindAllAvailableDonors();
                var donorsList = donors.ToList();
                
                // Log số lượng để kiểm tra
                Console.WriteLine($"Found {donorsList.Count} available donors");
                
                return Ok(new { 
                    donors = donorsList,
                    message = "Available donors search successful. Results are sorted by distance from reference point.",
                    currentUser = currentUser,
                    role = role,
                    searchCriteria = new {
                        profileStatusFilter = "Active",
                        appointmentStatusFilter = "Đã hiến",
                        referencePoint = "7 Đ. D1, Long Thạnh Mỹ, Thủ Đức, Hồ Chí Minh"
                    },
                    totalCount = donorsList.Count
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAllAvailableDonors: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Tìm kiếm tất cả người cần máu có điều kiện
        /// </summary>
        /// <remarks>
        /// API này trả về danh sách tất cả người cần máu có trạng thái "Đã xét duyệt".
        /// Điều kiện lọc:
        /// - Emergency.EmergencyStatus = "Đã xét duyệt"
        /// - Kết quả được sắp xếp theo khoảng cách từ điểm mốc: 7 Đ. D1, Long Thạnh Mỹ, Thủ Đức
        /// - Bao gồm thông tin bệnh viện và thông tin bệnh nhân
        /// </remarks>
        /// <returns>Danh sách tất cả người cần máu có điều kiện, sắp xếp theo khoảng cách</returns>
        [HttpGet("requests/all")]
        [Authorize(Roles = "Admin,Staff,User")]
        public async Task<IActionResult> GetAllApprovedBloodRequests()
        {
            try
            {
                // Lấy thông tin người dùng hiện tại từ token
                var currentUser = User.Identity?.Name ?? string.Empty;
                var role = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value ?? string.Empty;

                // Gọi service để lấy tất cả người cần máu có điều kiện
                var bloodRequests = await _searchService.FindAllApprovedBloodRequests();
                var requestsList = bloodRequests.ToList();
                
                // Log số lượng để kiểm tra
                Console.WriteLine($"Found {requestsList.Count} approved blood requests");
                
                return Ok(new { 
                    bloodRequests = requestsList,
                    message = "Approved blood requests search successful. Results are sorted by distance from reference point.",
                    currentUser = currentUser,
                    role = role,
                    searchCriteria = new {
                        emergencyStatusFilter = "Đã xét duyệt",
                        referencePoint = "7 Đ. D1, Long Thạnh Mỹ, Thủ Đức, Hồ Chí Minh"
                    },
                    totalCount = requestsList.Count
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAllApprovedBloodRequests: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
    }
}