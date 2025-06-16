using BloodDonationAPI.DTO;
using BloodDonationAPI.Entities;

namespace BloodDonationAPI.Service
{
    public interface IReportService
    {
        Task<List<ReportDto>> GetAllReports();
        Task<List<ReportDto>> GetUserReports(string username);
        Task<ReportDto> GetReportById(int reportId);
        Task<string> CreateReport(string username, CreateReportDto dto);
        Task<string> UpdateReport(int reportId, string username, UpdateReportDto dto);
        Task<string> DeleteReport(int reportId, string username);
    }
} 