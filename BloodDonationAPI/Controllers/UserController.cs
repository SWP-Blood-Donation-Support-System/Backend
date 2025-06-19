using BloodDonationAPI.DTO;
using BloodDonationAPI.Service;
using BloodDonationAPI.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BloodDonationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly JwtService _jwtService;
        private readonly BloodDonationSystemContext _context;

        public UserController(IUserService userService, JwtService jwtService, BloodDonationSystemContext context)
        {
            _userService = userService;
            _jwtService = jwtService;
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginDto)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == loginDto.Username);

                if (user == null || !VerifyPassword(loginDto.Password, user.Password))
                {
                    return Unauthorized(new { message = "Invalid username or password" });
                }

                var token = _jwtService.GenerateToken(user);

                return Ok(new
                {
                    token = token,
                    user = new
                    {
                        username = user.Username,
                        email = user.Email,
                        role = user.Role,
                        fullName = user.FullName
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        private bool VerifyPassword(string inputPassword, string? storedPassword)
        {
            if (storedPassword == null) return false;
            return inputPassword == storedPassword; // Đây chỉ là ví dụ, nên dùng proper password hashing
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = _userService.Register(registerDto);
            if (result == "Username already exists.")
            {
                return Conflict(result);
            }
            return Ok(result);
        }
    }
}
