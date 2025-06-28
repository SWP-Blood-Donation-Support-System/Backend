using System.Net;
using System.Net.Mail;

namespace BloodDonationAPI.Service
{
    public class EmailService : IEmailService
    {
        private const string EMAIL_USERNAME = "se183384lenguyenxuankhoi@gmail.com";
        private const string EMAIL_PASSWORD = "bdfepercxiktexac"; // KIỂM TRA KỸ - PHẢI 16 KÝ TỰ

        public async Task<bool> SendOtpEmailAsync(string toEmail, string otp)
        {
            Console.WriteLine($"🔄 Bắt đầu gửi email tới: {toEmail}");
            Console.WriteLine($"📧 Sử dụng email: {EMAIL_USERNAME}");
            Console.WriteLine($"🔑 Mật khẩu length: {EMAIL_PASSWORD.Length}");
            
            try
            {
                using var smtpClient = new SmtpClient("smtp.gmail.com", 587);
                smtpClient.EnableSsl = true;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new NetworkCredential(EMAIL_USERNAME, EMAIL_PASSWORD);
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.Timeout = 30000;

                Console.WriteLine("🔧 SMTP Client đã được cấu hình");

                using var mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(EMAIL_USERNAME, "Hệ thống Hiến máu");
                mailMessage.To.Add(toEmail);
                mailMessage.Subject = "Mã xác thực OTP - Hệ thống Hiến máu";
                mailMessage.Body = $@"
                    <html>
                    <body>
                        <h2>Xác thực tài khoản</h2>
                        <p>Chào bạn,</p>
                        <p>Mã OTP của bạn để hoàn tất đăng ký tài khoản là:</p>
                        <h1 style='color: #007bff; text-align: center; font-size: 36px; letter-spacing: 10px;'>{otp}</h1>
                        <p>Mã này có hiệu lực trong 5 phút.</p>
                        <p>Nếu bạn không yêu cầu tạo tài khoản, vui lòng bỏ qua email này.</p>
                        <br>
                        <p>Trân trọng,</p>
                        <p>Hệ thống Hiến máu</p>
                    </body>
                    </html>
                ";
                mailMessage.IsBodyHtml = true;

                Console.WriteLine("📝 Email message đã được tạo");
                Console.WriteLine("📤 Đang gửi email...");
                
                await smtpClient.SendMailAsync(mailMessage);
                
                Console.WriteLine($"✅ Email gửi thành công tới {toEmail}");
                return true;
            }
            catch (SmtpException smtpEx)
            {
                Console.WriteLine($"❌ SMTP Error: {smtpEx.Message}");
                Console.WriteLine($"❌ Status Code: {smtpEx.StatusCode}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ General Error: {ex.Message}");
                return false;
            }
        }
    }
}
