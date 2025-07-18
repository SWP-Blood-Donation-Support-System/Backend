using BloodDonationAPI.DTO;
using BloodDonationAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace BloodDonationAPI.Service
{
    public class DeferralReasonService : IDeferralReasonService
    {
        private readonly BloodDonationSystemContext _context;

    public DeferralReasonService(BloodDonationSystemContext context)
        {
            _context = context;
        }




        public async Task<bool> UpdateDeferralReasonAsync(UpdateDeferralReasonDto updateDeferralReasonDto)
        {
            var deferralReason = await _context.DeferralReasons.FindAsync(updateDeferralReasonDto.ReasonCode);
            if (deferralReason == null)
            {
                return false; // Deferral reason not found
            }
            deferralReason.ReasonText = updateDeferralReasonDto.ReasonText;
            deferralReason.MinDays = updateDeferralReasonDto.MinDays;
            deferralReason.IsPermanent = updateDeferralReasonDto.IsPermanent;
            deferralReason.Note = updateDeferralReasonDto.Note;
            deferralReason.MinHours = updateDeferralReasonDto.MinHours;
            deferralReason.MinMinutes = updateDeferralReasonDto.MinMinutes;

            _context.DeferralReasons.Update(deferralReason);
            await _context.SaveChangesAsync();
            return true; // Update successful
        }

        public async Task<List<DeferralReasonDto>> GetAllDeferralReasonsAsync()
        {
            return await _context.DeferralReasons
                .Select(dr => new DeferralReasonDto
                {
                    ReasonCode = dr.ReasonCode,
                    ReasonText = dr.ReasonText,
                    MinDays = dr.MinDays,
                    IsPermanent = dr.IsPermanent,
                    Note = dr.Note,
                    MinHours = dr.MinHours,
                    MinMinutes = dr.MinMinutes
                }).ToListAsync();
        }

        public async Task<ServiceResultDto> AddDeferralReasonAsync(DeferralReasonDto createDto)
        {
            if (string.IsNullOrWhiteSpace(createDto.ReasonCode))
            {
                return new ServiceResultDto { Success = false, Message = "Reason code is required." };
            }
            if(await _context.DeferralReasons.AnyAsync(dr => dr.ReasonCode == createDto.ReasonCode))
            {
                return new ServiceResultDto { Success = false, Message = "Deferral reason with this code already exists." };
            }
            var deferralReason = new DeferralReason
            {
                ReasonCode = createDto.ReasonCode,
                ReasonText = createDto.ReasonText,
                MinDays = createDto.MinDays,
                IsPermanent = createDto.IsPermanent,
                Note = createDto.Note,
                MinHours = createDto.MinHours,
                MinMinutes = createDto.MinMinutes
            };
            _context.DeferralReasons.Add(deferralReason);
            await _context.SaveChangesAsync();
            return new ServiceResultDto { Success = true, Message = "Deferral reason added successfully." };
        }

        public async Task<bool> DeleteDeferralReasonAsync(string reasonCode)
        {
            var deferralReason = await _context.DeferralReasons.FindAsync(reasonCode);
            if (deferralReason == null)
            {
                return false; // Deferral reason not found
            }
            _context.DeferralReasons.Remove(deferralReason);
            await _context.SaveChangesAsync();
            return true; // Deletion successful
        }

    }

    
}
