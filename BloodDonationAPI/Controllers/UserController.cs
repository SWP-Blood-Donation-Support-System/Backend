using BloodDonationAPI.DTO;
using BloodDonationAPI.Service;
using BloodDonationAPI.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace BloodDonationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly JwtService _jwtService;
        private readonly BloodDonationSystemContext _context;
        private readonly IGoogleAuthService _googleAuthService;

        public UserController(IUserService userService, JwtService jwtService, BloodDonationSystemContext context, IGoogleAuthService googleAuthService)
        {
            _userService = userService;
            _jwtService = jwtService;
            _context = context;
            _googleAuthService = googleAuthService;
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

        /// <summary>
        /// Đăng nhập bằng tài khoản Google
        /// </summary>
        /// <param name="googleLoginDto">Thông tin đăng nhập Google</param>
        /// <returns>JWT token và thông tin người dùng</returns>
        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDto googleLoginDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _googleAuthService.GoogleLoginAsync(googleLoginDto);

                if (result.IsNewUser)
                {
                    return Ok(new
                    {
                        token = result.Token,
                        isNewUser = result.IsNewUser,
                        message = result.Message,
                        user = result.User,
                        additionalInfo = "Vui lòng hoàn thành thông tin cá nhân để sử dụng đầy đủ các tính năng"
                    });
                }
                else
                {
                    return Ok(new
                    {
                        token = result.Token,
                        isNewUser = result.IsNewUser,
                        message = result.Message,
                        user = result.User
                    });
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Cập nhật thông tin profile người dùng
        /// </summary>
        /// <param name="updateProfileDto">Thông tin cập nhật</param>
        /// <returns>Thông tin người dùng đã cập nhật</returns>
        [HttpPut("update-profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto updateProfileDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Lấy username từ token
                var username = User.Identity?.Name;
                if (string.IsNullOrEmpty(username))
                {
                    return Unauthorized(new { message = "Invalid token" });
                }

                // Tìm user trong database
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                // Cập nhật thông tin
                if (updateProfileDto.DateOfBirth.HasValue)
                    user.DateOfBirth = updateProfileDto.DateOfBirth;
                
                if (!string.IsNullOrWhiteSpace(updateProfileDto.Gender))
                    user.Gender = updateProfileDto.Gender;
                
                if (!string.IsNullOrWhiteSpace(updateProfileDto.Phone))
                    user.Phone = updateProfileDto.Phone;
                
                if (!string.IsNullOrWhiteSpace(updateProfileDto.Address))
                    user.Address = updateProfileDto.Address;
                
                if (!string.IsNullOrWhiteSpace(updateProfileDto.BloodType))
                {
                    // Validate blood type
                    var validBloodTypes = new[] { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" };
                    if (validBloodTypes.Contains(updateProfileDto.BloodType.ToUpper()))
                    {
                        user.BloodType = updateProfileDto.BloodType.ToUpper();
                    }
                    else
                    {
                        return BadRequest(new { message = "Invalid blood type. Must be A+, A-, B+, B-, AB+, AB-, O+, or O-" });
                    }
                }
                
                if (!string.IsNullOrWhiteSpace(updateProfileDto.ProfileStatus))
                    user.ProfileStatus = updateProfileDto.ProfileStatus;
                
                if (!string.IsNullOrWhiteSpace(updateProfileDto.FullName))
                    user.FullName = updateProfileDto.FullName;

                // Kiểm tra nếu profile đã hoàn thành
                if (IsProfileComplete(user))
                {
                    user.ProfileStatus = "Sẵn sàng hiến máu";
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Profile updated successfully",
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
                        profileStatus = user.ProfileStatus
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        private bool IsProfileComplete(User user)
        {
            return !string.IsNullOrWhiteSpace(user.FullName) &&
                   !string.IsNullOrWhiteSpace(user.Gender) &&
                   !string.IsNullOrWhiteSpace(user.Phone) &&
                   !string.IsNullOrWhiteSpace(user.Address) &&
                   !string.IsNullOrWhiteSpace(user.BloodType) &&
                   user.DateOfBirth.HasValue;
        }

        /// <summary>
        /// Lấy thông tin profile người dùng hiện tại
        /// </summary>
        /// <returns>Thông tin người dùng</returns>
        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                // Lấy username từ token
                var username = User.Identity?.Name;
                if (string.IsNullOrEmpty(username))
                {
                    return Unauthorized(new { message = "Invalid token" });
                }

                // Tìm user trong database
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                return Ok(new
                {
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
                        profileStatus = user.ProfileStatus
                    },
                    isProfileComplete = IsProfileComplete(user)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
        
    }
}
