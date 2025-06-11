using BloodDonationAPI.DTOs;

namespace BloodDonationAPI.Service
{
    public interface IDonorSearchService
    {
        Task<DonorSearchResponse> FindNearbyDonorsAsync(DonorSearchRequest request);
    }
}