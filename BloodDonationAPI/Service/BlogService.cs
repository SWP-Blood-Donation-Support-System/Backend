using BloodDonationAPI.DTO;
using BloodDonationAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace BloodDonationAPI.Service
{
    public class BlogService : IBlogService
    {
        private readonly BloodDonationSystemContext _context;
        private readonly ILogger<BlogService> _logger;

        public BlogService(BloodDonationSystemContext context, ILogger<BlogService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<BlogDto>> GetAllBlogs()
        {
            try
            {
                return await _context.Blogs
                    .Where(b => b.BlogStatus == "available")
                    .Include(b => b.UsernameNavigation)
                    .Select(b => new BlogDto
                    {
                        BlogId = b.BlogId,
                        BlogTitle = b.BlogTitle,
                        BlogContent = b.BlogContent,
                        BlogImage = b.BlogImage,
                        BlogStatus = b.BlogStatus,
                        BlogDetail = b.BlogDetail,
                        Username = b.Username,
                        AuthorName = b.UsernameNavigation.FullName
                    })
                    .OrderByDescending(b => b.BlogId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all blogs");
                throw;
            }
        }

        public async Task<BlogDto?> GetBlogById(int id)
        {
            try
            {
                var blog = await _context.Blogs
                    .Include(b => b.UsernameNavigation)
                    .FirstOrDefaultAsync(b => b.BlogId == id);

                if (blog == null)
                    return null;

                return new BlogDto
                {
                    BlogId = blog.BlogId,
                    BlogTitle = blog.BlogTitle,
                    BlogContent = blog.BlogContent,
                    BlogImage = blog.BlogImage,
                    BlogStatus = blog.BlogStatus,
                    BlogDetail = blog.BlogDetail,
                    Username = blog.Username,
                    AuthorName = blog.UsernameNavigation.FullName
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting blog by id");
                throw;
            }
        }

        public async Task<string> CreateBlog(string username, CreateBlogDto dto)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
                if (user == null)
                    return "User not found.";

                var blog = new Blog
                {
                    BlogTitle = dto.BlogTitle,
                    BlogContent = dto.BlogContent,
                    BlogImage = dto.BlogImage,
                    BlogStatus = "available",
                    BlogDetail = dto.BlogDetail,
                    Username = username
                };

                _context.Blogs.Add(blog);
                await _context.SaveChangesAsync();

                return "Blog created successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating blog");
                throw;
            }
        }

        public async Task<string> UpdateBlog(int id, string username, string role, UpdateBlogDto dto)
        {
            try
            {
                var blog = await _context.Blogs.FindAsync(id);
                if (blog == null)
                    return "Blog not found.";

                if (role != "Admin" && blog.Username != username)
                    return "You are not authorized to update this blog.";

                blog.BlogTitle = dto.BlogTitle;
                blog.BlogContent = dto.BlogContent;
                blog.BlogImage = dto.BlogImage;
                blog.BlogStatus = dto.BlogStatus;
                blog.BlogDetail = dto.BlogDetail;

                await _context.SaveChangesAsync();
                return "Blog updated successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating blog");
                throw;
            }
        }

        public async Task<string> DeleteBlog(int id, string username, string role)
        {
            try
            {
                var blog = await _context.Blogs.FindAsync(id);
                if (blog == null)
                    return "Blog not found.";

                if (role != "Admin" && blog.Username != username)
                    return "You are not authorized to delete this blog.";

                _context.Blogs.Remove(blog);
                await _context.SaveChangesAsync();

                return "Blog deleted successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting blog");
                throw;
            }
        }
    }
} 