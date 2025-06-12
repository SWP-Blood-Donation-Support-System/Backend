using BloodDonationAPI.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BloodDonationAPI.Controllers
{    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BloodRequestsController : ControllerBase
    {
        private readonly BloodDonationSystemContext _context;

        public BloodRequestsController(BloodDonationSystemContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Tìm kiếm người cần máu theo nhóm máu
        /// </summary>
        /// <param name="bloodType">Nhóm máu (A+, A-, B+, B-, AB+, AB-, O+, O-)</param>
        /// <returns>Danh sách người cần máu có nhóm máu đã chọn</returns>
        [HttpGet("findByBloodType")]
        [AllowAnonymous] // Allow anonymous access to this specific endpoint
        public async Task<IActionResult> GetEmergenciesByBloodType([FromQuery] string bloodType)
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

                // Get all emergencies
                var allEmergencies = await _context.Emergencies.ToListAsync();
                
                // Debug: Log the total number of emergencies found
                Console.WriteLine($"Total emergencies found: {allEmergencies.Count}");
                
                // Filter by blood type
                var matchingEmergencies = allEmergencies
                    .Where(e => e.BloodType == normalizedBloodType)
                    .Select(e => new {
                        e.Username,
                        EmergencyDate = e.EmergencyDate,
                        BloodType = e.BloodType,
                        EmergencyStatus = e.EmergencyStatus,
                        EmergencyNote = e.EmergencyNote,
                        RequiredUnits = e.RequiredUnits,
                        HospitalId = e.HospitalId
                    })
                    .ToList();
                
                // Debug: Log the number of matching emergencies found
                Console.WriteLine($"Matching emergencies found: {matchingEmergencies.Count}");

                // Get hospital information for each emergency
                var hospitals = await _context.Hospitals.ToListAsync();
                
                var result = matchingEmergencies.Select(e => {
                    var hospital = hospitals.FirstOrDefault(h => h.HospitalId == e.HospitalId);
                    return new {
                        e.Username,
                        e.EmergencyDate,
                        e.BloodType,
                        e.EmergencyStatus,
                        e.EmergencyNote,
                        e.RequiredUnits,
                        e.HospitalId,
                        HospitalName = hospital?.HospitalName,
                        HospitalAddress = hospital?.HospitalAddress,
                        HospitalPhone = hospital?.HospitalPhone
                    };
                }).ToList();
                
                return Ok(new { 
                    emergencies = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }        // No nearby search functionality - removed as requested
    }
}