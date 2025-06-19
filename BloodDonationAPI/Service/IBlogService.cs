using BloodDonationAPI.DTO;
using BloodDonationAPI.Entities;

namespace BloodDonationAPI.Service
{
    public interface IBlogService
    {
        Task<List<BlogDto>> GetAllBlogs();
        Task<BlogDto?> GetBlogById(int id);
        Task<string> CreateBlog(string username, CreateBlogDto dto);
        Task<string> UpdateBlog(int id, string username, UpdateBlogDto dto);
        Task<string> DeleteBlog(int id, string username);
    }
} 