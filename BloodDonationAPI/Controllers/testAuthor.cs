using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodDonationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestAuthController : ControllerBase
    {
        // 1. API không cần đăng nhập - ai cũng gọi được
        [AllowAnonymous]
        [HttpGet("public")]
        public IActionResult PublicEndpoint()
        {
            return Ok(new { message = "This is public API - No login required!" });
        }

        // 2. API cần đăng nhập - user nào cũng gọi được miễn là đã đăng nhập
        [Authorize]
        [HttpGet("user")]
        public IActionResult UserOnlyEndpoint()
        {
            return Ok(new 
            { 
                message = "This is protected API - Login required!",
                currentUser = User.Identity?.Name
            });
        }

        // 3. API chỉ dành cho Admin
        [Authorize(Roles = "Admin")]
        [HttpGet("admin")]
        public IActionResult AdminOnlyEndpoint()
        {
            return Ok(new 
            { 
                message = "This is admin API - Only admin can access!",
                currentUser = User.Identity?.Name,
                role = "Admin"
            });
        }

        // 4. API dành cho Staff hoặc Admin
        [Authorize(Roles = "Staff,Admin")]
        [HttpGet("staff")]
        public IActionResult StaffEndpoint()
        {
            return Ok(new 
            { 
                message = "This is staff API - Only staff and admin can access!",
                currentUser = User.Identity?.Name,
                role = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value
            });
        }

        // 5. API trả về thông tin người dùng hiện tại
        [Authorize]
        [HttpGet("me")]
        public IActionResult GetCurrentUser()
        {
            return Ok(new
            {
                username = User.Identity?.Name,
                role = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value,
                claims = User.Claims.Select(c => new { c.Type, c.Value })
            });
        }
    }
}
