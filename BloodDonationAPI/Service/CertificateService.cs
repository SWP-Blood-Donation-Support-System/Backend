using BloodDonationAPI.DTO;
using BloodDonationAPI.Entities;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Runtime.ConstrainedExecution;
namespace BloodDonationAPI.Service
{
    public class CertificateService : ICertificateService
    {
        private readonly BloodDonationSystemContext _context;

        public CertificateService(BloodDonationSystemContext context)
        {
            _context = context;
        }
        public async Task<CertificateDto> GetCertificateAsync(int appointmentId)
        {
            var certificate = await _context.Certificates
                .Include(c => c.Appointment)
                .FirstOrDefaultAsync(c => c.AppointmentId == appointmentId);

            if (certificate == null) return null;

            return new CertificateDto
            {
                CertificateId = certificate.CertificateId,
                AppointmentId = certificate.AppointmentId,
                FullName = certificate.FullName,
                DateOfBirth = certificate.DateOfBirth,
                Address = certificate.Address,
                HospitalName = certificate.HospitalName,
                BloodAmount = certificate.BloodAmount,
                DonationDate = certificate.DonationDate,
                CertificateCode = certificate.CertificateCode,
                IssueDate = certificate.IssueDate,

            };
        }
        public byte[] GenerateCertificatePdf(CertificateDto certificateDto)
        {
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.Content().Column(col =>
                    {
                        col.Item().Text("GIẤY CHỨNG NHẬN HIẾN MÁU")
                            .FontSize(24).Bold().AlignCenter();

                        col.Item().Text($"Họ tên: {certificateDto.FullName}").FontSize(14);
                        col.Item().Text($"Ngày sinh: {certificateDto.DateOfBirth:dd/MM/yyyy}");
                        col.Item().Text($"CCCD: {certificateDto.CertificateCode}");
                        col.Item().Text($"Địa chỉ: {certificateDto.Address}");
                        col.Item().Text($"Cơ sở hiến máu: {certificateDto.HospitalName}");
                        col.Item().Text($"Ngày hiến máu: {certificateDto.DonationDate:dd/MM/yyyy}");
                        col.Item().Text($"Lượng máu hiến: {certificateDto.BloodAmount} ml");
                        col.Item().Text($"Ngày cấp: {certificateDto.IssueDate:dd/MM/yyyy}");

                        col.Item().PaddingTop(40).AlignRight().Text("ĐẠI DIỆN CƠ SỞ HIẾN MÁU").Italic();
                    });

                });

            });
            return document.GeneratePdf();
        }
        }
   
}
