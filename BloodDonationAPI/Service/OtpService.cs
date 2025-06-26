using System.Collections.Concurrent;
using BloodDonationAPI.DTO;

namespace BloodDonationAPI.Service
{
    public class OtpService : IOtpService
    {
        private readonly ConcurrentDictionary<string, (string otp, DateTime expiry, RegisterDto registerData)> _otpStorage 
            = new ConcurrentDictionary<string, (string, DateTime, RegisterDto)>();

        public string GenerateOtp()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        public void StoreRegistrationData(string email, string otp, RegisterDto registerDto)
        {
            var expiry = DateTime.Now.AddMinutes(5); // OTP expires in 5 minutes
            _otpStorage.AddOrUpdate(email.ToLower(), (otp, expiry, registerDto), (key, value) => (otp, expiry, registerDto));
        }

        public bool VerifyOtp(string email, string otp)
        {
            var emailKey = email.ToLower();
            if (_otpStorage.TryGetValue(emailKey, out var storedData))
            {
                if (DateTime.Now <= storedData.expiry && storedData.otp == otp)
                {
                    return true;
                }
                // Remove expired OTP
                _otpStorage.TryRemove(emailKey, out _);
            }
            return false;
        }

        public RegisterDto? GetRegistrationData(string email, string otp)
        {
            var emailKey = email.ToLower();
            if (_otpStorage.TryGetValue(emailKey, out var storedData))
            {
                if (DateTime.Now <= storedData.expiry && storedData.otp == otp)
                {
                    return storedData.registerData;
                }
                // Remove expired OTP
                _otpStorage.TryRemove(emailKey, out _);
            }
            return null;
        }

        public void RemoveOtp(string email)
        {
            _otpStorage.TryRemove(email.ToLower(), out _);
        }
    }
}
