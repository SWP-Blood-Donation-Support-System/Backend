using BloodDonationAPI.DTO;
using BloodDonationAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace BloodDonationAPI.Service
{
    public class ReportService : IReportService
    {
        private readonly BloodDonationSystemContext _context;
        private readonly ILogger<ReportService> _logger;

        public ReportService(BloodDonationSystemContext context, ILogger<ReportService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<ReportDto>> GetAllReports()
        {
            try
            {
                return await _context.Reports
                    .Select(r => new ReportDto
                    {
                        ReportId = r.ReportId,
                        Username = r.Username,
                        ReportDate = r.ReportDate,
                        ReportType = r.ReportType,
                        ReportContent = r.ReportContent
                    })
                    .OrderByDescending(r => r.ReportDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all reports");
                throw;
            }
        }

        public async Task<List<ReportDto>> GetUserReports(string username)
        {
            try
            {
                return await _context.Reports
                    .Where(r => r.Username == username)
                    .Select(r => new ReportDto
                    {
                        ReportId = r.ReportId,
                        Username = r.Username,
                        ReportDate = r.ReportDate,
                        ReportType = r.ReportType,
                        ReportContent = r.ReportContent
                    })
                    .OrderByDescending(r => r.ReportDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user reports");
                throw;
            }
        }

        public async Task<ReportDto> GetReportById(int reportId)
        {
            try
            {
                var report = await _context.Reports.FindAsync(reportId);
                if (report == null)
                    return null;

                return new ReportDto
                {
                    ReportId = report.ReportId,
                    Username = report.Username,
                    ReportDate = report.ReportDate,
                    ReportType = report.ReportType,
                    ReportContent = report.ReportContent
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting report by id");
                throw;
            }
        }

        public async Task<string> CreateReport(string username, CreateReportDto dto)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
                if (user == null)
                    return "User not found.";

                if (user.ProfileStatus != "Active")
                    return "User profile is not active.";

                var report = new Report
                {
                    Username = username,
                    ReportDate = DateOnly.FromDateTime(DateTime.Now),
                    ReportType = dto.ReportType,
                    ReportContent = dto.ReportContent
                };

                _context.Reports.Add(report);
                await _context.SaveChangesAsync();

                return "Report created successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating report");
                throw;
            }
        }

        public async Task<string> UpdateReport(int reportId, string username, UpdateReportDto dto)
        {
            try
            {
                var report = await _context.Reports.FindAsync(reportId);
                if (report == null)
                    return "Report not found.";

                if (report.Username != username)
                    return "You are not authorized to update this report.";

                report.ReportType = dto.ReportType;
                report.ReportContent = dto.ReportContent;

                await _context.SaveChangesAsync();
                return "Report updated successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating report");
                throw;
            }
        }

        public async Task<string> DeleteReport(int reportId, string username)
        {
            try
            {
                var report = await _context.Reports.FindAsync(reportId);
                if (report == null)
                    return "Report not found.";

                if (report.Username != username)
                    return "You are not authorized to delete this report.";

                _context.Reports.Remove(report);
                await _context.SaveChangesAsync();

                return "Report deleted successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting report");
                throw;
            }
        }
    }
} 