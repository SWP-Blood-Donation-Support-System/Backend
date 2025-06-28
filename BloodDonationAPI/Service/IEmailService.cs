namespace BloodDonationAPI.Service
{
    public interface IEmailService
    {
        Task<bool> SendOtpEmailAsync(string toEmail, string otp);
    }
}
