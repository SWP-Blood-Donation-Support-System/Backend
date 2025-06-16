using BloodDonationAPI.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BloodDonationAPI.Service
{
    public class SearchService : ISearchService
    {
        private readonly BloodDonationSystemContext _context;

        public SearchService(BloodDonationSystemContext context)
        {
            _context = context;
        }
        
        /// <summary>
        /// Tìm kiếm người hiến máu theo nhóm máu
        /// </summary>
        /// <param name="bloodType">Nhóm máu cần tìm</param>
        /// <returns>Danh sách người hiến máu phù hợp</returns>
        public async Task<IEnumerable<object>> FindDonorsByBloodType(string bloodType)
        {
            if (string.IsNullOrEmpty(bloodType))
                throw new ArgumentException("Blood type is required", nameof(bloodType));

            var normalizedBloodType = bloodType.ToUpper().Trim();
            var compatibleDonorTypes = GetCompatibleDonorTypes()[normalizedBloodType];
            
            var donors = await _context.Users.ToListAsync();
            var compatibleDonors = donors
                .Where(u => u.BloodType != null && 
                           compatibleDonorTypes.Contains(u.BloodType) && 
                           u.ProfileStatus == "Active")
                .Select(u => new {
                    FullName = u.FullName, 
                    Email = u.Email,
                    DateOfBirth = u.DateOfBirth,
                    Gender = u.Gender,
                    Phone = u.Phone,
                    Address = u.Address,
                    BloodType = u.BloodType,
                    Distance = CalculateDistanceFromAddress(u.Address ?? string.Empty)
                })
                .OrderBy(u => u.Distance)
                .ToList();
                
            return compatibleDonors;
        }

        /// <summary>
        /// Tìm kiếm các trường hợp khẩn cấp theo nhóm máu
        /// </summary>
        /// <param name="bloodType">Nhóm máu cần tìm</param>
        /// <returns>Danh sách các trường hợp khẩn cấp</returns>
        public async Task<IEnumerable<object>> FindEmergenciesByBloodType(string bloodType)
        {
            if (string.IsNullOrEmpty(bloodType))
                throw new ArgumentException("Blood type is required", nameof(bloodType));
                
            var normalizedBloodType = bloodType.ToUpper().Trim();
            
            // Get all emergencies with status "Đã xét duyệt"
            var allEmergencies = await _context.Emergencies
                .Where(e => e.EmergencyStatus == "Đã xét duyệt")
                .ToListAsync();
            
            // Filter by blood type
            var matchingEmergencies = allEmergencies
                .Where(e => e.BloodType == normalizedBloodType)
                .Select(e => new {
                    e.Username,
                    EmergencyDate = e.EmergencyDate,
                    BloodType = e.BloodType,
                    EmergencyStatus = e.EmergencyStatus,
                    EmergencyNote = e.EmergencyNote,
                    RequiredUnits = e.RequiredUnits,
                    HospitalId = e.HospitalId
                })
                .ToList();

            // Get hospital information for each emergency
            var hospitals = await _context.Hospitals.ToListAsync();
              var result = matchingEmergencies.Select(e => {
                var hospital = hospitals.FirstOrDefault(h => h.HospitalId == e.HospitalId);
                return new {
                    Username = e.Username ?? string.Empty,
                    EmergencyDate = e.EmergencyDate,
                    BloodType = e.BloodType ?? string.Empty,
                    EmergencyStatus = e.EmergencyStatus ?? string.Empty,
                    EmergencyNote = e.EmergencyNote ?? string.Empty,
                    RequiredUnits = e.RequiredUnits,
                    HospitalId = e.HospitalId,
                    HospitalName = hospital?.HospitalName ?? string.Empty,
                    HospitalAddress = hospital?.HospitalAddress ?? string.Empty,
                    HospitalPhone = hospital?.HospitalPhone ?? string.Empty
                };
            }).ToList<object>();
            
            return result;
        }

        /// <summary>
        /// Tìm kiếm tất cả các trường hợp khẩn cấp
        /// </summary>
        /// <returns>Danh sách tất cả các trường hợp khẩn cấp</returns>
        public async Task<IEnumerable<object>> FindAllEmergencies()
        {
            // Get all emergencies
            var allEmergencies = await _context.Emergencies.ToListAsync();
            
            // Get hospital information for each emergency
            var hospitals = await _context.Hospitals.ToListAsync();
            
            var result = allEmergencies.Select(e => {
                var hospital = hospitals.FirstOrDefault(h => h.HospitalId == e.HospitalId);
                return new {
                    e.Username,
                    EmergencyDate = e.EmergencyDate,
                    BloodType = e.BloodType,
                    EmergencyStatus = e.EmergencyStatus,
                    EmergencyNote = e.EmergencyNote,
                    RequiredUnits = e.RequiredUnits,
                    e.HospitalId,
                    HospitalName = hospital?.HospitalName,
                    HospitalAddress = hospital?.HospitalAddress,
                    HospitalPhone = hospital?.HospitalPhone
                };
            }).ToList<object>();
            
            return result;
        }

        /// <summary>
        /// Tính khoảng cách dựa trên địa chỉ
        /// </summary>
        /// <param name="address">Địa chỉ cần tính khoảng cách</param>
        /// <returns>Khoảng cách tính bằng số</returns>
        private double CalculateDistanceFromAddress(string address)
        {
            // Đảm bảo address không null
            address = address ?? string.Empty;
            
            if (string.IsNullOrEmpty(address))
                return double.MaxValue; // Địa chỉ trống sẽ hiển thị cuối cùng
            
            // Các quận trung tâm TP.HCM
            if (address.Contains("Quận 1") || address.Contains("Q1") ||
                address.Contains("Quận 3") || address.Contains("Q3") ||
                address.Contains("Quận 4") || address.Contains("Q4") ||
                address.Contains("Quận 5") || address.Contains("Q5") ||
                address.Contains("Quận 10") || address.Contains("Q10"))
            {
                return 1; // Ưu tiên các quận trung tâm
            }
            
            // Các quận lân cận trung tâm
            if (address.Contains("Quận 2") || address.Contains("Q2") ||
                address.Contains("Quận 6") || address.Contains("Q6") ||
                address.Contains("Quận 7") || address.Contains("Q7") ||
                address.Contains("Phú Nhuận") ||
                address.Contains("Bình Thạnh"))
            {
                return 2;
            }
            
            // Các quận khác trong TP.HCM
            if (address.Contains("TP.HCM") || address.Contains("TP. HCM") || 
                address.Contains("HCM") || address.Contains("Hồ Chí Minh") ||
                address.Contains("Tân Bình") || address.Contains("Gò Vấp") ||
                address.Contains("Tân Phú") || address.Contains("Bình Tân") ||
                address.Contains("Thủ Đức"))
            {
                return 3;
            }
            
            // Các tỉnh lân cận
            if (address.Contains("Bình Dương") ||
                address.Contains("Đồng Nai") ||
                address.Contains("Long An") ||
                address.Contains("Vũng Tàu") ||
                address.Contains("Bà Rịa"))
            {
                return 4;
            }
            
            // Các tỉnh miền Nam
            if (address.Contains("Tiền Giang") ||
                address.Contains("Bến Tre") ||
                address.Contains("Cần Thơ") ||
                address.Contains("An Giang") ||
                address.Contains("Vĩnh Long") ||
                address.Contains("Đồng Tháp"))
            {
                return 5;
            }
            
            // Các tỉnh miền Trung
            if (address.Contains("Đà Nẵng") ||
                address.Contains("Huế") ||
                address.Contains("Quảng Nam") ||
                address.Contains("Quảng Ngãi") ||
                address.Contains("Nha Trang") ||
                address.Contains("Khánh Hòa"))
            {
                return 6;
            }
            
            // Các tỉnh miền Bắc
            if (address.Contains("Hà Nội") ||
                address.Contains("Hải Phòng") ||
                address.Contains("Quảng Ninh") ||
                address.Contains("Bắc Ninh") ||
                address.Contains("Hải Dương"))
            {
                return 7;
            }
            
            // Địa chỉ khác
            return 10;
        }

        /// <summary>
        /// Lấy danh sách các nhóm máu có thể hiến cho từng nhóm máu
        /// </summary>
        /// <returns>Từ điển mapping giữa nhóm máu với các nhóm máu tương thích</returns>
        private Dictionary<string, List<string>> GetCompatibleDonorTypes()
        {
            return new Dictionary<string, List<string>>
            {
                { "O-", new List<string> { "O-" } },
                { "O+", new List<string> { "O-", "O+" } },
                { "A-", new List<string> { "O-", "A-" } },
                { "A+", new List<string> { "O-", "O+", "A-", "A+" } },
                { "B-", new List<string> { "O-", "B-" } },
                { "B+", new List<string> { "O-", "O+", "B-", "B+" } },
                { "AB-", new List<string> { "O-", "A-", "B-", "AB-" } },
                { "AB+", new List<string> { "O-", "O+", "A-", "A+", "B-", "B+", "AB-", "AB+" } }
            };
        }
    }
}