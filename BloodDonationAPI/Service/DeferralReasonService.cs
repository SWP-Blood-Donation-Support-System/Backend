using BloodDonationAPI.DTO;

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
    }

    
}
