using BloodDonationAPI.DTO;
using BloodDonationAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace BloodDonationAPI.Service
{
    public class AppointmentServiece : IAppointmentServiece
    {
        private readonly BloodDonationSystemContext _context;
        public AppointmentServiece(BloodDonationSystemContext context)
        {
            _context = context;
        }
        public async Task<List<AppointmentList>> GetAppointmentLists()
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            return await _context.AppointmentLists
                .Where(a => a.AppointmentDate >= today)
                .ToListAsync();

        }
        public async Task<string> RegisterAppointment(string userName , RegisterAppointmentDto Dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == userName);
            if (user == null)
                return "User not found.";

            if (user.ProfileStatus != "Active")
                return "Bạn chưa đủ điều kiện để đăng ký.";

            var appointment = await _context.AppointmentLists
                .FirstOrDefaultAsync(a => a.AppointmentId == Dto.appointmentId);

            if (appointment == null)
                return "Lịch hẹn không tồn tại.";

            bool alreadyRegistered = await _context.AppointmentHistories.AnyAsync(h =>
                h.Username == userName && h.AppointmentId == Dto.appointmentId);

            if (alreadyRegistered)
                return "Bạn đã đăng ký lịch hẹn này rồi.";

            var history = new AppointmentHistory
            {
                Username = userName,
                AppointmentId = Dto.appointmentId,
                AppointmentDate = DateTime.Now,
                AppointmentStatus = "Đã đăng ký"
            };

            _context.AppointmentHistories.Add(history);
            await _context.SaveChangesAsync();

            return "Success";
        }
    
    }
}
