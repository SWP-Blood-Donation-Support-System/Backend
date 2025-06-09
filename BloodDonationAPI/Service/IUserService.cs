using BloodDonationAPI.DTO;
using BloodDonationAPI.Entities;

namespace BloodDonationAPI.Service
{
    public interface IUserService
    {
        User? Login(string username, string password);

        string Register(RegisterDto registerDto);
    }
    public class UserService : IUserService
    {
        private readonly BloodDonationSystemContext _context;

        public UserService(BloodDonationSystemContext context)
        {
            _context = context;
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
    }
}
    
