using BloodDonationAPI.DTO;

namespace BloodDonationAPI.Service
{
    public interface IOtpService
    {
        string GenerateOtp();
        void StoreRegistrationData(string email, string otp, RegisterDto registerDto);
        bool VerifyOtp(string email, string otp);
        RegisterDto? GetRegistrationData(string email, string otp);
        void RemoveOtp(string email);
    }
}
