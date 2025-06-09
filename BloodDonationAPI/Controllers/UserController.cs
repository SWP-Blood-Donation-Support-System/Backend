using BloodDonationAPI.DTO;
using BloodDonationAPI.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BloodDonationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequestDto loginDto)
        {
            var user = _userService.Login(loginDto.Username, loginDto.Password);
            if (user == null)
            {
                return Unauthorized("Invalid username or password.");
            }
            return Ok(new
            {
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                DateOfBirth = user.DateOfBirth,
            });
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
