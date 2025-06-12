using BloodDonationAPI.DTOs;
using BloodDonationAPI.Service;
using BloodDonationAPI.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BloodDonationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Keep general authorization for the controller
    public class DonorsController : ControllerBase
    {
        private readonly IDonorSearchService _donorSearchService;
        private readonly BloodDonationSystemContext _context;

        public DonorsController(IDonorSearchService donorSearchService, BloodDonationSystemContext context)
        {
            _donorSearchService = donorSearchService;
            _context = context;
        }        /// <summary>
        /// Tìm kiếm người hiến máu theo nhóm máu
        /// </summary>
        /// <param name="bloodType">Nhóm máu (A+, A-, B+, B-, AB+, AB-, O+, O-)</param>
        /// <returns>Danh sách người hiến máu có nhóm máu đã chọn, sắp xếp theo địa chỉ</returns>
        [HttpGet("findByBloodType")]
        [AllowAnonymous] // Allow anonymous access to this specific endpoint
        public async Task<IActionResult> GetDonorsByBloodType([FromQuery] string bloodType)
        {
            try
            {
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
                
                // Xác định danh sách các nhóm máu có thể hiến cho nhóm máu yêu cầu
                var compatibleDonorTypes = GetCompatibleDonorTypes()[normalizedBloodType];
                  // Use a simpler query approach to avoid SQL syntax issues
                var donors = await _context.Users.ToListAsync();
                
                // Debug: Log the total number of users found
                Console.WriteLine($"Total users found: {donors.Count}");
                
                // Check if there are any users with the compatible blood types
                var usersWithBloodType = donors.Where(u => u.BloodType != null).ToList();
                Console.WriteLine($"Users with blood type: {usersWithBloodType.Count}");
                  foreach (var donorType in compatibleDonorTypes)
                {
                    Console.WriteLine($"Looking for blood type: {donorType}");
                    var usersWithThisType = usersWithBloodType.Where(u => u.BloodType == donorType).ToList();
                    Console.WriteLine($"Found {usersWithThisType.Count} users with blood type {donorType}");
                }                // Remove filtering by role to see if that's causing the issue
                var compatibleDonors = donors
                    .Where(u => u.BloodType != null && compatibleDonorTypes.Contains(u.BloodType))
                    .Select(u => new {
                        Username = u.Username,
                        FullName = u.FullName, 
                        Email = u.Email,
                        Phone = u.Phone,
                        Address = u.Address,
                        DateOfBirth = u.DateOfBirth,
                        Gender = u.Gender,
                        BloodType = u.BloodType,
                        ProfileStatus = u.ProfileStatus,
                        Role = u.Role
                    })
                    .OrderBy(u => u.Address)
                    .ToList();
                
                // Debug: Log the number of compatible donors found
                Console.WriteLine($"Compatible donors found: {compatibleDonors.Count}");
                
                return Ok(new { 
                    donors = compatibleDonors
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // Helper method to extract distance from a dynamic object
        private double GetDistance(object donor)
        {
            try
            {
                var prop = donor.GetType().GetProperty("Distance");
                if (prop != null)
                {
                    var value = prop.GetValue(donor);
                    if (value != null)
                    {
                        return Convert.ToDouble(value);
                    }
                }
                
                // If the property is in a dictionary
                if (donor is IDictionary<string, object> dict)
                {
                    if (dict.ContainsKey("Distance"))
                    {
                        return Convert.ToDouble(dict["Distance"]);
                    }
                    // Try lowercase key
                    if (dict.ContainsKey("distance"))
                    {
                        return Convert.ToDouble(dict["distance"]);
                    }
                }
                
                return double.MaxValue; // Put at the end if no distance found
            }
            catch
            {
                return double.MaxValue;
            }
        }

        /// <summary>
        /// Get compatibility information for a specific blood type
        /// </summary>
        /// <param name="bloodType">Blood type to check compatibility for</param>
        /// <returns>Dictionary containing can donate to and can receive from information</returns>
        private dynamic GetBloodCompatibilityInfo(string bloodType)
        {
            var canDonateTo = GetCompatibleRecipients();
            var canReceiveFrom = GetCompatibleDonorTypes();
            
            return new
            {
                bloodType = bloodType,
                canDonateTo = canDonateTo.ContainsKey(bloodType) ? canDonateTo[bloodType] : new List<string>(),
                canReceiveFrom = canReceiveFrom.ContainsKey(bloodType) ? canReceiveFrom[bloodType] : new List<string>()
            };
        }
        
        /// <summary>
        /// Lấy danh sách các nhóm máu có thể hiến cho từng nhóm máu
        /// </summary>
        private Dictionary<string, List<string>> GetCompatibleDonorTypes()
        {
            return new Dictionary<string, List<string>>
            {
                { "O-", new List<string> { "O-" } },
                { "O+", new List<string> { "O-", "O+" } },
                { "A-", new List<string> { "O-", "A-" } },
                { "A+", new List<string> { "O-", "O+", "A-", "A+" } },
                { "B-", new List<string> { "O-", "B-" } },
                { "B+", new List<string> { "O-", "O+", "B-", "B+" } },
                { "AB-", new List<string> { "O-", "A-", "B-", "AB-" } },
                { "AB+", new List<string> { "O-", "O+", "A-", "A+", "B-", "B+", "AB-", "AB+" } }
            };
        }
        
        /// <summary>
        /// Lấy danh sách các nhóm máu mà từng nhóm máu có thể hiến cho
        /// </summary>
        private Dictionary<string, List<string>> GetCompatibleRecipients()
        {
            return new Dictionary<string, List<string>>
            {
                { "O-", new List<string> { "O-", "O+", "A-", "A+", "B-", "B+", "AB-", "AB+" } }, // Universal donor
                { "O+", new List<string> { "O+", "A+", "B+", "AB+" } },
                { "A-", new List<string> { "A-", "A+", "AB-", "AB+" } },
                { "A+", new List<string> { "A+", "AB+" } },
                { "B-", new List<string> { "B-", "B+", "AB-", "AB+" } },
                { "B+", new List<string> { "B+", "AB+" } },
                { "AB-", new List<string> { "AB-", "AB+" } },
                { "AB+", new List<string> { "AB+" } } // Universal recipient
            };
        }
    }
}