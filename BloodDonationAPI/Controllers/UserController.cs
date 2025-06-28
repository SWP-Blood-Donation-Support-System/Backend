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
                        fullName = user.FullName,
                        dateOfBirth = user.DateOfBirth,
                        gender = user.Gender,
                        phone = user.Phone,
                        address = user.Address,
                        bloodType = user.BloodType,

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

        // ENDPOINT 1: Đăng ký và gửi OTP
        [HttpPost("register")]
        public async Task<IActionResult> RegisterAndSendOtp([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _userService.RegisterAndSendOtpAsync(registerDto);
            
            if (result.Contains("gửi"))
            {
                return Ok(new { message = result });
            }
            
            return BadRequest(new { message = result });
        }

        // ENDPOINT 2: Xác thực OTP và tạo tài khoản (CHỈ CẦN EMAIL VÀ OTP)
        [HttpPost("verify-otp")]
        public IActionResult VerifyOtp([FromBody] VerifyOtpDto verifyOtpDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = _userService.VerifyOtpAndCreateAccount(verifyOtpDto);
            
            if (result.Contains("thành công"))
            {
                return Ok(new { message = result });
            }
            
            return BadRequest(new { message = result });
        }

        // ENDPOINT 3: Yêu cầu đặt lại mật khẩu (gửi OTP)
        [HttpPost("request-password-reset")]
        public async Task<IActionResult> RequestPasswordReset([FromBody] RequestPasswordResetDto requestDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _userService.RequestPasswordResetAsync(requestDto);
            
            if (result.Contains("gửi"))
            {
                return Ok(new { message = result });
            }
            
            return BadRequest(new { message = result });
        }

        // ENDPOINT 4: Đặt lại mật khẩu với OTP
        [HttpPost("reset-password")]
        public IActionResult ResetPassword([FromBody] ResetPasswordDto resetDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = _userService.ResetPassword(resetDto);
            
            if (result.Contains("thành công"))
            {
                return Ok(new { message = result });
            }
            
            return BadRequest(new { message = result });
        }
        
    }
}
