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
                    page.Size(PageSizes.A5.Landscape());
                    page.Margin(1, Unit.Centimetre);
                    page.Size(PageSizes.A5.Landscape());
                    page.Margin(1, Unit.Centimetre);

                   

                    page.Content().Row(row =>
                    {
                        // Cột trái: Hướng dẫn + KHUNG
                        row.ConstantItem(270)
                            .Border(1)
                            .BorderColor(Colors.Grey.Darken1)
                            .Padding(10)
                            .Background(Colors.White)
                            .Column(col =>
                            {
                                col.Item().AlignCenter().Text(t => t.Span("HIẾN MÁU CỨU NGƯỜI\nMỘT NGHĨA CỬ CAO ĐẸP").FontSize(14).Bold());

                                col.Item().PaddingVertical(5).Element(e => e.LineHorizontal(1).LineColor(Colors.Grey.Lighten2));

                                col.Item().Text(t =>
                                {
                                    t.Span("1. ").Bold().FontSize(10);
                                    t.Span("Giấy chứng nhận này được trao cho người hiến máu sau mỗi lần hiến máu tình nguyện.").FontSize(10);
                                });

                                col.Item().Text(t =>
                                {
                                    t.Span("2. ").Bold().FontSize(10);
                                    t.Span("Có giá trị để được truyền máu miễn phí bằng số lượng máu đã hiến, khi người hiến máu có nhu cầu sử dụng.").FontSize(10);
                                });

                                col.Item().Text(t =>
                                {
                                    t.Span("3. ").Bold().FontSize(10);
                                    t.Span("Người hiến máu cần xuất trình giấy chứng nhận này để được truyền máu miễn phí.").FontSize(10);
                                });

                                col.Item().Text(t =>
                                {
                                    t.Span("4. ").Bold().FontSize(10);
                                    t.Span("Cơ sở y tế có trách nhiệm xác nhận số lượng máu đã truyền miễn phí.").FontSize(10);
                                });

                                col.Item().PaddingTop(20).AlignCenter().Text(t => t.Span("CHỨNG NHẬN CỦA CƠ SỞ Y TẾ\nĐÃ TRUYỀN MÁU").Bold().FontSize(12));

                                col.Item().PaddingTop(10).Text("Ngày....... tháng....... năm........").FontSize(10);
                                col.Item().Text("Số lượng: ............. ml").FontSize(10);
                            });

                        // Cột phải: Giấy chứng nhận + KHUNG
                        row.RelativeItem()
                            .Border(1)
                            .BorderColor(Colors.Grey.Darken1)
                            .Padding(10)
                            .Background(Colors.White)
                            .PaddingLeft(20)
                            .Column(col =>
                            {
                                col.Item().AlignCenter().Text(t => t.Span("CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM").Bold().FontSize(10));
                                col.Item().PaddingBottom(10).AlignCenter().Text(t => t.Span("Độc lập - Tự do - Hạnh phúc").FontSize(10));

                                col.Item().PaddingBottom(10).AlignCenter().Text(t => t.Span("GIẤY CHỨNG NHẬN\nHIẾN MÁU TÌNH NGUYỆN").FontSize(16).Bold());

                                col.Item().Text(t => t.Span("Chứng nhận:").FontSize(12).Bold());

                                col.Item().Text(t => t.Span($"Ông/Bà: {certificateDto.FullName}").FontSize(11));
                                col.Item().Text(t => t.Span($"Sinh ngày: {certificateDto.DateOfBirth:dd/MM/yyyy}").FontSize(11));
                                col.Item().Text(t => t.Span($"Địa chỉ: {certificateDto.Address}").FontSize(11));

                                col.Item().PaddingTop(10).Text(t => t.Span("ĐÃ HIẾN MÁU TÌNH NGUYỆN").Bold().FontSize(12));

                                col.Item().Text(t => t.Span($"Tại: {certificateDto.HospitalName}").FontSize(11));

                                col.Item().Text(t =>
                                {
                                    var checkedBox = "[✔]";
                                    string option250 = certificateDto.BloodAmount == 250 ? $"{checkedBox} 250ml" : "[ ] 250ml";
                                    string option350 = certificateDto.BloodAmount == 350 ? $"{checkedBox} 350ml" : "[ ] 350ml";
                                    string option450 = certificateDto.BloodAmount == 450 ? $"{checkedBox} 450ml" : "[ ] 450ml";

                                    t.Span("Số lượng: ").FontSize(11);
                                    t.Span($"{option250}   {option350}   {option450}").FontSize(11);
                                });

                                col.Item().Text(t => t.Span($"Ngày hiến máu: {certificateDto.DonationDate:dd/MM/yyyy}").FontSize(11));

                                col.Item().PaddingTop(20).Row(row2 =>
                                {
                                    row2.RelativeItem().Text(""); // khoảng trắng bên trái
                                    row2.ConstantItem(200).Column(innerCol =>
                                    {
                                        innerCol.Item().AlignCenter().Text(t => t.Span("TM. BAN VẬN ĐỘNG").Bold().FontSize(10));
                                        innerCol.Item().AlignCenter().Text(t => t.Span("HIẾN MÁU TÌNH NGUYỆN").Bold().FontSize(10));
                                        innerCol.Item().PaddingTop(40).AlignCenter().Text(t => t.Span("Chữ ký / đóng dấu").Italic().FontSize(10));
                                    });
                                });

                                col.Item().PaddingTop(20).AlignRight().Text(t => t.Span($"Số: {certificateDto.CertificateCode}").FontSize(10));
                            });
                    });
                });
            });

            return document.GeneratePdf();
        }


    }

}
