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

        public async Task<bool> AddDonationHistoryAsync(CreateDonationHistoryDto registrationDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == registrationDto.Username);
            if (user == null)
                return false;
            var donationHistory = new DonationHistory
            {
                Username = registrationDto.Username,
                BloodType = registrationDto.BloodType,
                DonationDate = registrationDto.DonationDate,
                DonationStatus = registrationDto.DonationStatus,
                DonationUnit = registrationDto.DonationUnit
            };

            _context.DonationHistories.Add(donationHistory);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<BloodBank> AddBloodToBankAsync(AddBloodBankDto dto)
        {
            var donation = await _context.DonationHistories.FirstOrDefaultAsync(h => h.DonationHistoryId == dto.DonationHistoryId);
            if (donation == null)
                return null;
            var bloodBank = new BloodBank
            {
                
                BloodTypeName = dto.BloodTypeName,
                Unit = dto.Unit,
                DonationHistoryId = dto.DonationHistoryId,
                ExpiryDate = dto.ExpiryDate ,
                Status = string.IsNullOrWhiteSpace(dto.Status) ? "storing" : dto.Status.Trim().ToLower()
            };
            _context.BloodBanks.Add(bloodBank);

            donation.DonationStatus = "stored"; // Update donation status to 'stored'
            _context.DonationHistories.Update(donation);
            await _context.SaveChangesAsync();
            return bloodBank;


        }

        public async Task<List<DonationHistoryDto>> GetDonationHistoryByUserNameAsync(string username)
        {
            return await _context.DonationHistories
                .Where(d => d.Username == username)
                .Select(d => new DonationHistoryDto
                {
                    DonationHistoryId = d.DonationHistoryId,
                    Username = d.Username,
                    BloodType = d.BloodType,
                    DonationDate = d.DonationDate,
                    DonationStatus = d.DonationStatus,
                    DonationUnit = d.DonationUnit
                })
                .OrderByDescending(d => d.DonationDate)
                .ToListAsync();
        }
    }
}
