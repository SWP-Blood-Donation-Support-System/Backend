using BloodDonationAPI.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace BloodDonationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HospitalController : ControllerBase
    {
        private readonly BloodDonationSystemContext _context;
        public HospitalController(BloodDonationSystemContext context)
        {
            _context = context;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAllHospitals()
        {
            var hospitals = await _context.Hospitals.ToListAsync();
            return Ok(hospitals);
        }

        [HttpPost("Create")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> CreateHospital([FromBody] Hospital hospital)
        {
            if (hospital == null)
                return BadRequest("Invalid hospital data.");
            _context.Hospitals.Add(hospital);
            await _context.SaveChangesAsync();
            return Ok(hospital);
        }

        [HttpPut("Update/{id}")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> UpdateHospital(int id, [FromBody] Hospital updatedHospital)
        {
            var hospital = await _context.Hospitals.FindAsync(id);
            if (hospital == null)
                return NotFound("Hospital not found.");
            hospital.HospitalName = updatedHospital.HospitalName;
            hospital.HospitalAddress = updatedHospital.HospitalAddress;
            hospital.HospitalImage = updatedHospital.HospitalImage;
            hospital.HospitalPhone = updatedHospital.HospitalPhone;
            await _context.SaveChangesAsync();
            return Ok(hospital);
        }

        [HttpDelete("Delete/{id}")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> DeleteHospital(int id)
        {
            var hospital = await _context.Hospitals.FindAsync(id);
            if (hospital == null)
                return NotFound("Hospital not found.");
            _context.Hospitals.Remove(hospital);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Hospital deleted successfully." });
        }
    }
} 