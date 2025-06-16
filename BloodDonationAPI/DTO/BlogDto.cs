namespace BloodDonationAPI.DTO
{
    public class BlogDto
    {
        public int BlogId { get; set; }
        public string BlogTitle { get; set; } = null!;
        public string BlogContent { get; set; } = null!;
        public string? BlogImage { get; set; }
        public string Username { get; set; } = null!;
        public string? AuthorName { get; set; }
    }

    public class CreateBlogDto
    {
        public string BlogTitle { get; set; } = null!;
        public string BlogContent { get; set; } = null!;
        public string? BlogImage { get; set; }
    }

    public class UpdateBlogDto
    {
        public string BlogTitle { get; set; } = null!;
        public string BlogContent { get; set; } = null!;
        public string? BlogImage { get; set; }
    }
} 