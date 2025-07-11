using BloodDonationAPI.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BloodDonationAPI.Service
{    
    /// <summary>
    /// Interface cho dịch vụ tìm kiếm người hiến máu và các trường hợp khẩn cấp
    /// </summary>
    public interface ISearchService
    {
        /// <summary>
        /// Tìm kiếm người hiến máu theo nhóm máu
        /// </summary>
        /// <param name="bloodType">Nhóm máu cần tìm</param>
        /// <returns>Danh sách người hiến máu phù hợp</returns>
        Task<IEnumerable<object>> FindDonorsByBloodType(string bloodType);
        
        /// <summary>
        /// Tìm kiếm các trường hợp khẩn cấp theo nhóm máu
        /// </summary>
        /// <param name="bloodType">Nhóm máu cần tìm</param>
        /// <returns>Danh sách các trường hợp khẩn cấp</returns>
        Task<IEnumerable<object>> FindEmergenciesByBloodType(string bloodType);
        
        /// <summary>
        /// Tìm kiếm tất cả các trường hợp khẩn cấp
        /// </summary>
        /// <returns>Danh sách tất cả các trường hợp khẩn cấp</returns>
        Task<IEnumerable<object>> FindAllEmergencies();

        /// <summary>
        /// Tìm kiếm yêu cầu máu theo nhóm máu
        /// </summary>
        /// <param name="bloodType">Nhóm máu cần tìm</param>
        /// <returns>Danh sách yêu cầu máu được sắp xếp theo khoảng cách</returns>
        Task<IEnumerable<object>> FindBloodRequestsByBloodType(string bloodType);

        /// <summary>
        /// Tìm kiếm tất cả người hiến máu có điều kiện
        /// </summary>
        /// <returns>Danh sách tất cả người hiến máu đã hoàn thành hiến máu và có trạng thái Active</returns>
        Task<IEnumerable<object>> FindAllAvailableDonors();

        /// <summary>
        /// Tìm kiếm tất cả người cần máu có điều kiện
        /// </summary>
        /// <returns>Danh sách tất cả người cần máu có trạng thái Đã xét duyệt</returns>
        Task<IEnumerable<object>> FindAllApprovedBloodRequests();
    }
}