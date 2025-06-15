namespace BloodDonationAPI.DTO
{
    public class ReportDto
    {
        public int ReportId { get; set; }
        public string Username { get; set; }
        public DateOnly? ReportDate { get; set; }
        public string ReportType { get; set; }
        public string ReportContent { get; set; }
    }

    public class CreateReportDto
    {
        public string ReportType { get; set; }
        public string ReportContent { get; set; }
    }

    public class UpdateReportDto
    {
        public string ReportType { get; set; }
        public string ReportContent { get; set; }
    }
} 