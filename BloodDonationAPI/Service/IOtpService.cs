using BloodDonationAPI.DTO;

namespace BloodDonationAPI.Service
{
    public interface IOtpService
    {
        string GenerateOtp();
        void StoreRegistrationData(string email, string otp, RegisterDto registerDto);
        bool VerifyOtp(string otp);
        RegisterDto? GetRegistrationDataByOtp(string otp);
        void RemoveOtpByCode(string otp);
        
        // Methods for password reset
        void StorePasswordResetData(string email, string otp, string username);
        string? GetUsernameByPasswordResetOtp(string otp);
        bool VerifyPasswordResetOtp(string otp);
        void RemovePasswordResetOtp(string otp);
    }
}
