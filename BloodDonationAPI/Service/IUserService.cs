using BloodDonationAPI.Entities;

namespace BloodDonationAPI.Service
{
    public interface IUserService
    {
        User? Login(string username, string password);
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
    }
    }
