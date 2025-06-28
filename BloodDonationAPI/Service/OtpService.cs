using System.Collections.Concurrent;
using BloodDonationAPI.DTO;

namespace BloodDonationAPI.Service
{
    public class OtpService : IOtpService
    {
        private readonly ConcurrentDictionary<string, (string email, DateTime expiry, RegisterDto registerData)> _otpStorage 
            = new ConcurrentDictionary<string, (string, DateTime, RegisterDto)>();
        
        private readonly ConcurrentDictionary<string, (string email, DateTime expiry, string username)> _passwordResetOtpStorage 
            = new ConcurrentDictionary<string, (string, DateTime, string)>();

        public string GenerateOtp()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        public void StoreRegistrationData(string email, string otp, RegisterDto registerDto)
        {
            var expiry = DateTime.Now.AddMinutes(5);
            // Lưu theo key là OTP thay vì email
            _otpStorage.AddOrUpdate(otp, (email, expiry, registerDto), (key, value) => (email, expiry, registerDto));
        }

        public bool VerifyOtp(string otp)
        {
            if (_otpStorage.TryGetValue(otp, out var storedData))
            {
                if (DateTime.Now <= storedData.expiry)
                {
                    return true;
                }
                // Remove expired OTP
                _otpStorage.TryRemove(otp, out _);
            }
            return false;
        }

        public RegisterDto? GetRegistrationDataByOtp(string otp)
        {
            if (_otpStorage.TryGetValue(otp, out var storedData))
            {
                if (DateTime.Now <= storedData.expiry)
                {
                    return storedData.registerData;
                }
                // Remove expired OTP
                _otpStorage.TryRemove(otp, out _);
            }
            return null;
        }

        public void RemoveOtpByCode(string otp)
        {
            _otpStorage.TryRemove(otp, out _);
        }

        // Password reset methods
        public void StorePasswordResetData(string email, string otp, string username)
        {
            var expiry = DateTime.Now.AddMinutes(5);
            _passwordResetOtpStorage.AddOrUpdate(otp, (email, expiry, username), (key, value) => (email, expiry, username));
        }

        public string? GetUsernameByPasswordResetOtp(string otp)
        {
            if (_passwordResetOtpStorage.TryGetValue(otp, out var storedData))
            {
                if (DateTime.Now <= storedData.expiry)
                {
                    return storedData.username;
                }
                // Remove expired OTP
                _passwordResetOtpStorage.TryRemove(otp, out _);
            }
            return null;
        }

        public bool VerifyPasswordResetOtp(string otp)
        {
            if (_passwordResetOtpStorage.TryGetValue(otp, out var storedData))
            {
                if (DateTime.Now <= storedData.expiry)
                {
                    return true;
                }
                // Remove expired OTP
                _passwordResetOtpStorage.TryRemove(otp, out _);
            }
            return false;
        }

        public void RemovePasswordResetOtp(string otp)
        {
            _passwordResetOtpStorage.TryRemove(otp, out _);
        }
    }
}
