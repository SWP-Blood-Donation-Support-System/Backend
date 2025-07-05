using BloodDonationAPI.DTO;
using BloodDonationAPI.DTOs;

namespace BloodDonationAPI.Service
{
    public interface IDonorSearchService
    {
        /// <summary>
        /// Tìm kiếm người hiến máu gần kề dựa trên vị trí
        /// </summary>
        /// <param name="request">Yêu cầu tìm kiếm chứa thông tin vị trí và nhóm máu</param>
        /// <returns>Danh sách người hiến máu gần kề</returns>
        Task<DonorSearchResponseDTO> FindNearbyDonorsAsync(DonorSearchRequestDTO request);
        
        /// <summary>
        /// Tìm kiếm người hiến máu theo nhóm máu
        /// </summary>
        /// <param name="request">Yêu cầu tìm kiếm chứa thông tin nhóm máu</param>
        /// <returns>Danh sách người hiến máu có nhóm máu phù hợp</returns>
        Task<DonorSearchResponseDTO> FindDonorsByBloodTypeAsync(DonorSearchRequestDTO request);
        
        /// <summary>
        /// Tìm kiếm người hiến máu trong khu vực Hồ Chí Minh theo nhóm máu
        /// </summary>
        /// <param name="request">Yêu cầu tìm kiếm chứa thông tin nhóm máu</param>
        /// <returns>Danh sách người hiến máu ở HCM có nhóm máu phù hợp</returns>
        Task<DonorSearchResponseDTO> FindDonorsInHCMByBloodTypeAsync(DonorSearchRequestDTO request);
    }
}