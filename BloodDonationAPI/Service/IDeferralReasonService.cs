using BloodDonationAPI.DTO;

namespace BloodDonationAPI.Service
{
    public interface IDeferralReasonService
    {
        Task<bool> UpdateDeferralReasonAsync(UpdateDeferralReasonDto updateDeferralReasonDto);
    }
}
