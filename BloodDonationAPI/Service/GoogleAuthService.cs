using Google.Apis.Auth;
using BloodDonationAPI.DTO;
using BloodDonationAPI.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace BloodDonationAPI.Service
{
    public interface IGoogleAuthService
    {
        Task<GoogleLoginResponseDto> GoogleLoginAsync(GoogleLoginDto googleLoginDto);
    }

    public class GoogleAuthService : IGoogleAuthService
    {
        private readonly BloodDonationSystemContext _context;
        private readonly JwtService _jwtService;
        private readonly IConfiguration _configuration;

        public GoogleAuthService(BloodDonationSystemContext context, JwtService jwtService, IConfiguration configuration)
        {
            _context = context;
            _jwtService = jwtService;
            _configuration = configuration;
        }

        public async Task<GoogleLoginResponseDto> GoogleLoginAsync(GoogleLoginDto googleLoginDto)
        {
            try
            {
                // Verify Google token
                var googleUser = await VerifyGoogleTokenAsync(googleLoginDto.GoogleToken);
                
                if (googleUser == null)
                {
                    throw new UnauthorizedAccessException("Invalid Google token");
                }

                // Check if user already exists
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == googleUser.Email);

                if (existingUser != null)
                {
                    // User exists, generate JWT token and return
                    var token = _jwtService.GenerateToken(existingUser);
                    
                    return new GoogleLoginResponseDto
                    {
                        Token = token,
                        IsNewUser = false,
                        Message = "Đăng nhập thành công",
                        User = new UserInfo
                        {
                            Username = existingUser.Username,
                            Email = existingUser.Email ?? "",
                            Role = existingUser.Role ?? "",
                            FullName = existingUser.FullName ?? "",
                            DateOfBirth = existingUser.DateOfBirth,
                            Gender = existingUser.Gender ?? "",
                            Phone = existingUser.Phone ?? "",
                            Address = existingUser.Address ?? "",
                            BloodType = existingUser.BloodType ?? "",
                            ProfileStatus = existingUser.ProfileStatus ?? ""
                        }
                    };
                }
                else
                {
                    // User doesn't exist, create new user
                    var newUser = await CreateGoogleUserAsync(googleUser);
                    var token = _jwtService.GenerateToken(newUser);
                    
                    return new GoogleLoginResponseDto
                    {
                        Token = token,
                        IsNewUser = true,
                        Message = "Tài khoản mới đã được tạo thành công",
                        User = new UserInfo
                        {
                            Username = newUser.Username,
                            Email = newUser.Email ?? "",
                            Role = newUser.Role ?? "",
                            FullName = newUser.FullName ?? "",
                            DateOfBirth = newUser.DateOfBirth,
                            Gender = newUser.Gender ?? "",
                            Phone = newUser.Phone ?? "",
                            Address = newUser.Address ?? "",
                            BloodType = newUser.BloodType ?? "",
                            ProfileStatus = newUser.ProfileStatus ?? ""
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Google login failed: {ex.Message}");
            }
        }

        private async Task<GoogleUserInfo?> VerifyGoogleTokenAsync(string token)
        {
            try
            {
                // Get Google Client ID from configuration
                var googleClientId = _configuration["GoogleAuth:ClientId"];
                
                if (string.IsNullOrEmpty(googleClientId))
                {
                    throw new Exception("Google Client ID not configured");
                }

                var payload = await GoogleJsonWebSignature.ValidateAsync(token, new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { googleClientId }
                });

                return new GoogleUserInfo
                {
                    Sub = payload.Subject,
                    Email = payload.Email,
                    Name = payload.Name,
                    Picture = payload.Picture,
                    EmailVerified = payload.EmailVerified,
                    GivenName = payload.GivenName,
                    FamilyName = payload.FamilyName
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Google token verification failed: {ex.Message}");
                return null;
            }
        }

        private async Task<User> CreateGoogleUserAsync(GoogleUserInfo googleUser)
        {
            // Generate unique username based on email
            var username = await GenerateUniqueUsernameAsync(googleUser.Email);
            
            var newUser = new User
            {
                Username = username,
                Email = googleUser.Email,
                FullName = googleUser.Name,
                Role = "User", // Default role for Google users
                ProfileStatus = "Sẵn sàng hiến máu", // 🆕 Thay đổi từ "Chưa hoàn thành" thành "Sẵn sàng hiến máu"
                UserStatus = "Active", // 🆕 Thêm UserStatus mặc định
                Password = GenerateRandomPassword(), // Generate random password for Google users
                Gender = null, // Will be filled later by user
                DateOfBirth = null, // Will be filled later by user
                Phone = null, // Will be filled later by user
                Address = null, // Will be filled later by user
                BloodType = null // Will be filled later by user
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return newUser;
        }

        private async Task<string> GenerateUniqueUsernameAsync(string email)
        {
            // Extract the local part of the email (before @)
            var baseUsername = email.Split('@')[0];
            
            // Remove invalid characters
            baseUsername = new string(baseUsername.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());
            
            // Ensure it's not empty
            if (string.IsNullOrEmpty(baseUsername))
            {
                baseUsername = "user";
            }

            var username = baseUsername;
            var counter = 1;

            // Check if username already exists and generate a unique one
            while (await _context.Users.AnyAsync(u => u.Username == username))
            {
                username = $"{baseUsername}{counter}";
                counter++;
            }

            return username;
        }

        private string GenerateRandomPassword()
        {
            // Generate a random password for Google users (they won't use it anyway)
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[32];
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }
    }
}
