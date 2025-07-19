using BloodDonationAPI.DTO;

namespace BloodDonationAPI.Service
{
    public interface IDeferralReasonService
    {
        Task<bool> UpdateDeferralReasonAsync(UpdateDeferralReasonDto updateDeferralReasonDto);
        Task<List<DeferralReasonDto>> GetAllDeferralReasonsAsync();

        Task<ServiceResultDto> AddDeferralReasonAsync(DeferralReasonDto createDto);


        Task<bool> DeleteDeferralReasonAsync(string reasonCode);
    }
}
