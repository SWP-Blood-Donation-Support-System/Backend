using BloodDonationAPI.DTO;
using BloodDonationAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace BloodDonationAPI.Service
{
    public class BloodDonationProcessService : IBloodDonationProcessService
    {
        private readonly BloodDonationSystemContext _context;

        public BloodDonationProcessService(BloodDonationSystemContext context)
        {
            _context = context;
        }

        public async Task<List<AppointmentRegistrationDto>> GetRegistrationsByAppointmentID(int AppointmentID)
        {
            return await _context.AppointmentHistories
                 .Where(h => h.AppointmentId == AppointmentID)
                 .Include(h => h.UsernameNavigation)
                 .Select(h => new AppointmentRegistrationDto
                 {
                     AppointmentHistoryId = h.AppointmentHistoryId,
                     Username = h.Username,
                     FullName = h.UsernameNavigation.FullName,
                     Phone = h.UsernameNavigation.Phone,
                     AppointmentDate = h.AppointmentDate,
                     AppointmentStatus = h.AppointmentStatus
                 })
                 .ToListAsync();

        }

        public async Task<bool> UpdateAppointmentStatusAsync(UpdateAppointmentStatusDto updateDto)
        {
            var history = await _context.AppointmentHistories.FirstOrDefaultAsync(h => h.AppointmentHistoryId == updateDto.AppointmentHistoryId);
            if (history == null)
                return false;

            history.AppointmentStatus = updateDto.AppointmentStatus;
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
