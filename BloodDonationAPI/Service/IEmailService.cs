using BloodDonationAPI.DTO;

namespace BloodDonationAPI.Service
{
    public interface IEmailService
    {
        Task<bool> SendOtpEmailAsync(string toEmail, string otp);
        Task<bool> SendEventReminderEmailAsync(EventReminderDto reminderDto);
    }
}
