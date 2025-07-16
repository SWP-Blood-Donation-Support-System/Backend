using System.Net;
using System.Net.Mail;
using BloodDonationAPI.DTO;

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

        public async Task<bool> SendEventReminderEmailAsync(EventReminderDto reminderDto)
        {
            Console.WriteLine($"🔄 Bắt đầu gửi email nhắc nhở event tới: {reminderDto.UserEmail}");
            Console.WriteLine($"📅 Event: {reminderDto.EventTitle} - {reminderDto.EventDate}");
            
            try
            {
                using var smtpClient = new SmtpClient("smtp.gmail.com", 587);
                smtpClient.EnableSsl = true;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new NetworkCredential(EMAIL_USERNAME, EMAIL_PASSWORD);
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.Timeout = 30000;

                Console.WriteLine("🔧 SMTP Client đã được cấu hình cho email nhắc nhở");

                using var mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(EMAIL_USERNAME, "Hệ thống Hiến máu");
                mailMessage.To.Add(reminderDto.UserEmail);
                mailMessage.Subject = $"Nhắc nhở: Sự kiện hiến máu - {reminderDto.EventTitle}";
                
                var eventDateTime = reminderDto.EventDate.ToString("dd/MM/yyyy");
                var eventTime = reminderDto.EventTime.ToString("HH:mm");
                
                mailMessage.Body = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif; color: #333;'>
                        <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px;'>
                            <h2 style='color: #d32f2f; text-align: center; margin-bottom: 30px;'>
                                🩸 Nhắc nhở sự kiện hiến máu
                            </h2>
                            
                            <p style='font-size: 16px; margin-bottom: 20px;'>
                                Chào <strong>{reminderDto.UserFullName}</strong>,
                            </p>
                            
                            <p style='font-size: 16px; margin-bottom: 20px;'>
                                Đây là thông báo nhắc nhở về sự kiện hiến máu mà bạn đã đăng ký:
                            </p>
                            
                            <div style='background-color: #f5f5f5; padding: 20px; border-radius: 8px; margin: 20px 0;'>
                                <h3 style='color: #d32f2f; margin-top: 0;'>📋 Thông tin sự kiện:</h3>
                                <p><strong>🎯 Tên sự kiện:</strong> {reminderDto.EventTitle}</p>
                                <p><strong>📅 Ngày:</strong> {eventDateTime}</p>
                                <p><strong>⏰ Giờ:</strong> {eventTime}</p>
                                <p><strong>📍 Địa điểm:</strong> {reminderDto.Location}</p>
                                {(!string.IsNullOrEmpty(reminderDto.BloodTypeRequired) ? $"<p><strong>🩸 Nhóm máu cần:</strong> {reminderDto.BloodTypeRequired}</p>" : "")}
                            </div>
                            
                            <div style='background-color: #fff3cd; padding: 15px; border-radius: 8px; border-left: 4px solid #ffc107; margin: 20px 0;'>
                                <h4 style='color: #856404; margin-top: 0;'>📝 Nội dung sự kiện:</h4>
                                <p style='color: #856404; margin-bottom: 0;'>{reminderDto.EventContent}</p>
                            </div>
                            
                            <div style='background-color: #d4edda; padding: 15px; border-radius: 8px; border-left: 4px solid #28a745; margin: 20px 0;'>
                                <h4 style='color: #155724; margin-top: 0;'>⚠️ Lưu ý quan trọng:</h4>
                                <ul style='color: #155724; margin-bottom: 0;'>
                                    <li>Vui lòng có mặt đúng giờ tại địa điểm</li>
                                    <li>Mang theo chứng minh nhân dân/căn cước công dân</li>
                                    <li>Ăn uống đầy đủ trước khi hiến máu</li>
                                    <li>Không uống rượu bia 24h trước khi hiến máu</li>
                                    <li>Ngủ đủ giấc và giữ sức khỏe tốt</li>
                                </ul>
                            </div>
                            
                            <div style='text-align: center; margin: 30px 0;'>
                                <p style='font-size: 18px; color: #d32f2f; font-weight: bold;'>
                                    🤝 Cảm ơn bạn đã tham gia hoạt động hiến máu cứu người!
                                </p>
                            </div>
                            
                            <hr style='border: none; border-top: 1px solid #e0e0e0; margin: 30px 0;'>
                            
                            <p style='font-size: 14px; color: #666; text-align: center; margin-bottom: 0;'>
                                Trân trọng,<br>
                                <strong>Hệ thống Hiến máu</strong><br>
                                <em>Email này được gửi tự động, vui lòng không trả lời.</em>
                            </p>
                        </div>
                    </body>
                    </html>
                ";
                mailMessage.IsBodyHtml = true;

                Console.WriteLine("📝 Email nhắc nhở đã được tạo");
                Console.WriteLine("📤 Đang gửi email nhắc nhở...");
                
                await smtpClient.SendMailAsync(mailMessage);
                
                Console.WriteLine($"✅ Email nhắc nhở gửi thành công tới {reminderDto.UserEmail}");
                return true;
            }
            catch (SmtpException smtpEx)
            {
                Console.WriteLine($"❌ SMTP Error khi gửi email nhắc nhở: {smtpEx.Message}");
                Console.WriteLine($"❌ Status Code: {smtpEx.StatusCode}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ General Error khi gửi email nhắc nhở: {ex.Message}");
                return false;
            }
        }
    }
}
