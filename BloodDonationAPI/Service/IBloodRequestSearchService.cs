using BloodDonationAPI.DTO;
using BloodDonationAPI.DTOs;
using System.Threading.Tasks;

namespace BloodDonationAPI.Service
{
    // This interface is no longer needed, but keeping it for reference
    public interface IBloodRequestSearchService
    {
        /// <summary>
        /// Tìm kiếm yêu cầu máu gần kề
        /// </summary>
        /// <param name="request">Thông tin tìm kiếm</param>
        /// <returns>Danh sách yêu cầu máu phù hợp với tiêu chí tìm kiếm</returns>
        Task<BloodRequestSearchResponseDTO> FindNearbyBloodRequests(BloodRequestSearchRequestDTO request);
    }
}