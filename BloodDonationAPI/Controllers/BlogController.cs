using BloodDonationAPI.DTO;
using BloodDonationAPI.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BloodDonationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogController : ControllerBase
    {
        private readonly IBlogService _blogService;
        private readonly ILogger<BlogController> _logger;

        public BlogController(IBlogService blogService, ILogger<BlogController> logger)
        {
            _blogService = blogService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách tất cả các blog
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllBlogs()
        {
            try
            {
                var blogs = await _blogService.GetAllBlogs();
                return Ok(blogs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all blogs");
                return StatusCode(500, new { message = "An error occurred while processing your request.", error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy thông tin blog theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBlogById(int id)
        {
            try
            {
                var blog = await _blogService.GetBlogById(id);
                if (blog == null)
                    return NotFound(new { message = "Blog not found." });

                return Ok(blog);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting blog by id");
                return StatusCode(500, new { message = "An error occurred while processing your request.", error = ex.Message });
            }
        }

        /// <summary>
        /// Tạo blog mới
        /// </summary>
        [HttpPost("CreateBlog")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> CreateBlog([FromBody] CreateBlogDto dto)
        {
            try
            {
                var username = User.FindFirst(ClaimTypes.Name)?.Value;
                if (username == null)
                    return Unauthorized(new { message = "User not authenticated." });

                var result = await _blogService.CreateBlog(username, dto);
                if (result == "Blog created successfully.")
                    return Ok(new { message = result });

                return BadRequest(new { message = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating blog");
                return StatusCode(500, new { message = "An error occurred while processing your request.", error = ex.Message });
            }
        }

        /// <summary>
        /// Cập nhật blog theo ID
        /// </summary>
        [HttpPut("UpdateBlog/{id}")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> UpdateBlog(int id, [FromBody] UpdateBlogDto dto)
        {
            try
            {
                var username = User.FindFirst(ClaimTypes.Name)?.Value;
                if (username == null)
                    return Unauthorized(new { message = "User not authenticated." });

                var result = await _blogService.UpdateBlog(id, username, dto);
                if (result == "Blog updated successfully.")
                    return Ok(new { message = result });

                return BadRequest(new { message = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating blog");
                return StatusCode(500, new { message = "An error occurred while processing your request.", error = ex.Message });
            }
        }

        /// <summary>
        /// Xóa blog theo ID
        /// </summary>
        [HttpDelete("DeleteBlog/{id}")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> DeleteBlog(int id)
        {
            try
            {
                var username = User.FindFirst(ClaimTypes.Name)?.Value;
                if (username == null)
                    return Unauthorized(new { message = "User not authenticated." });

                var result = await _blogService.DeleteBlog(id, username);
                if (result == "Blog deleted successfully.")
                    return Ok(new { message = result });

                return BadRequest(new { message = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting blog");
                return StatusCode(500, new { message = "An error occurred while processing your request.", error = ex.Message });
            }
        }
    }
} 