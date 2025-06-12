using BloodDonationAPI.DTOs;
using System.Threading.Tasks;

namespace BloodDonationAPI.Service
{
    // This interface is no longer needed, but keeping it for reference
    public interface IBloodRequestSearchService
    {
        // Nearby search functionality has been removed
        Task<BloodRequestSearchResponse> FindNearbyBloodRequests(BloodRequestSearchRequest request);
    }
}