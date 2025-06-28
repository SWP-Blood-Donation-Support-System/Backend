using BloodDonationAPI.DTO;
using BloodDonationAPI.Entities;

namespace BloodDonationAPI.Service
{
    public interface IUserService
    {
        User? Login(string username, string password);
        string Register(RegisterDto registerDto);
        Task<string> RegisterAndSendOtpAsync(RegisterDto registerDto);
        string VerifyOtpAndCreateAccount(VerifyOtpDto verifyOtpDto);
        
        // Password reset methods
        Task<string> RequestPasswordResetAsync(RequestPasswordResetDto requestDto);
        string ResetPassword(ResetPasswordDto resetDto);
    }
    
    public class UserService : IUserService
    {
        private readonly BloodDonationSystemContext _context;
        private readonly IEmailService _emailService;
        private readonly IOtpService _otpService;

        public UserService(BloodDonationSystemContext context, IEmailService emailService, IOtpService otpService)
        {
            _context = context;
            _emailService = emailService;
            _otpService = otpService;
        }

        public User? Login(string username, string password)
        {
            return _context.Users.FirstOrDefault(u => u.Username == username && u.Password == password);
        }

        public string Register(RegisterDto registerDto)
        {
            if (_context.Users.Any(u => u.Username == registerDto.Username))
            {
                return "Username already exists.";
            }
            var user = new User
            {
                Username = registerDto.Username,
                Password = registerDto.Password,  
                Email = registerDto.Email,
                FullName = registerDto.FullName,
                DateOfBirth = registerDto.DateOfBirth,
                Gender = registerDto.Gender,
                Phone = registerDto.Phone,
                Address = registerDto.Address,
                BloodType = registerDto.BloodTypeId,
                ProfileStatus = "Active",
                Role = "User"
            };
            _context.Users.Add(user);
            _context.SaveChanges();
            return "Registration successful.";
        }

        public async Task<string> RegisterAndSendOtpAsync(RegisterDto registerDto)
        {
            // Check if email already exists
            if (_context.Users.Any(u => u.Email == registerDto.Email))
            {
                return "Email đã được sử dụng để đăng ký tài khoản khác.";
            }

            // Check if username already exists
            if (_context.Users.Any(u => u.Username == registerDto.Username))
            {
                return "Tên đăng nhập đã tồn tại.";
            }

            // Generate OTP
            var otp = _otpService.GenerateOtp();
            
            // Store registration data with OTP
            _otpService.StoreRegistrationData(registerDto.Email!, otp, registerDto);

            // Send OTP via email
            var emailSent = await _emailService.SendOtpEmailAsync(registerDto.Email!, otp);
            
            if (emailSent)
            {
                return "Mã OTP đã được gửi tới email của bạn. Vui lòng kiểm tra hộp thư và nhập mã OTP để hoàn tất đăng ký.";
            }
            else
            {
                _otpService.RemoveOtpByCode(otp);
                return "Không thể gửi email. Vui lòng thử lại sau.";
            }
        }

        public string VerifyOtpAndCreateAccount(VerifyOtpDto verifyOtpDto)
        {
            // Get registration data chỉ bằng OTP
            var storedRegisterDto = _otpService.GetRegistrationDataByOtp(verifyOtpDto.Otp);
            
            if (storedRegisterDto == null)
            {
                return "Mã OTP không đúng hoặc đã hết hạn.";
            }

            // Double check if email or username already exists
            if (_context.Users.Any(u => u.Email == storedRegisterDto.Email))
            {
                _otpService.RemoveOtpByCode(verifyOtpDto.Otp);
                return "Email đã được sử dụng để đăng ký tài khoản khác.";
            }

            if (_context.Users.Any(u => u.Username == storedRegisterDto.Username))
            {
                _otpService.RemoveOtpByCode(verifyOtpDto.Otp);
                return "Tên đăng nhập đã tồn tại.";
            }

            // Create user account
            var user = new User
            {
                Username = storedRegisterDto.Username,
                Password = storedRegisterDto.Password,
                Email = storedRegisterDto.Email,
                FullName = storedRegisterDto.FullName,
                DateOfBirth = storedRegisterDto.DateOfBirth,
                Gender = storedRegisterDto.Gender,
                Phone = storedRegisterDto.Phone,
                Address = storedRegisterDto.Address,
                BloodType = storedRegisterDto.BloodTypeId,
                ProfileStatus = "Active",
                Role = "User"
            };

            _context.Users.Add(user);
            _context.SaveChanges();
            
            // Remove OTP after successful registration
            _otpService.RemoveOtpByCode(verifyOtpDto.Otp);
            
            return "Đăng ký tài khoản thành công.";
        }

        public async Task<string> RequestPasswordResetAsync(RequestPasswordResetDto requestDto)
        {
            // Find user by email or username
            var user = _context.Users.FirstOrDefault(u => 
                u.Email == requestDto.EmailOrUsername || 
                u.Username == requestDto.EmailOrUsername);

            if (user == null)
            {
                return "Không tìm thấy tài khoản với email hoặc tên đăng nhập này.";
            }

            // Generate OTP
            var otp = _otpService.GenerateOtp();
            
            // Store password reset data with OTP
            _otpService.StorePasswordResetData(user.Email!, otp, user.Username);

            // Send OTP via email
            var emailSent = await _emailService.SendOtpEmailAsync(user.Email!, otp);
            
            if (emailSent)
            {
                return "Mã OTP đã được gửi tới email của bạn. Vui lòng kiểm tra hộp thư và nhập mã OTP để đặt lại mật khẩu.";
            }
            else
            {
                _otpService.RemovePasswordResetOtp(otp);
                return "Không thể gửi email. Vui lòng thử lại sau.";
            }
        }

        public string ResetPassword(ResetPasswordDto resetDto)
        {
            // Get username by OTP
            var username = _otpService.GetUsernameByPasswordResetOtp(resetDto.Otp);
            
            if (username == null)
            {
                return "Mã OTP không đúng hoặc đã hết hạn.";
            }

            // Find user
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            
            if (user == null)
            {
                _otpService.RemovePasswordResetOtp(resetDto.Otp);
                return "Không tìm thấy tài khoản.";
            }

            // Update password
            user.Password = resetDto.NewPassword;
            _context.SaveChanges();
            
            // Remove OTP after successful password reset
            _otpService.RemovePasswordResetOtp(resetDto.Otp);
            
            return "Đặt lại mật khẩu thành công.";
        }
    }
}