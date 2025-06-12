using BloodDonationAPI.DTOs;

namespace BloodDonationAPI.Service
{
    public interface IDonorSearchService
    {
        Task<DonorSearchResponse> FindNearbyDonorsAsync(DonorSearchRequest request);
        Task<DonorSearchResponse> FindDonorsByBloodTypeAsync(DonorSearchRequest request);
        Task<DonorSearchResponse> FindDonorsInHCMByBloodTypeAsync(DonorSearchRequest request);
    }
}