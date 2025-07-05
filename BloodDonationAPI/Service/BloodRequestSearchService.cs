using BloodDonationAPI.DTO;
using BloodDonationAPI.DTOs;
using BloodDonationAPI.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BloodDonationAPI.Service
{
    public class BloodRequestSearchService : IBloodRequestSearchService
    {
        private readonly BloodDonationSystemContext _context;
        // Reference point: 7 Đ. D1, Long Thạnh Mỹ, Thủ Đức, Hồ Chí Minh 700000, Vietnam
        private readonly double _referenceLatitude = 10.841962;  // Approximate latitude for the reference point
        private readonly double _referenceLongitude = 106.810627; // Approximate longitude for the reference point

        public BloodRequestSearchService(BloodDonationSystemContext context)
        {
            _context = context;
        }

        public async Task<BloodRequestSearchResponseDTO> FindNearbyBloodRequests(BloodRequestSearchRequestDTO request)
        {
            // Validate the request parameters
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var response = new BloodRequestSearchResponseDTO();
            
            try
            {
                // Lấy các yêu cầu máu từ bảng Emergency
                var emergencies = await _context.Emergencies
                    .Where(e => string.IsNullOrEmpty(request.BloodType) || 
                               (e.BloodType != null && e.BloodType == request.BloodType))
                    .ToListAsync();
                
                var mockRequests = new List<BloodRequestResult>();
                
                foreach (var emergency in emergencies)
                {
                    // Lấy thông tin người dùng
                    var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == emergency.Username);
                    
                    // Lấy thông tin bệnh viện
                    var hospital = await _context.Hospitals.FirstOrDefaultAsync(h => h.HospitalId == emergency.HospitalId);
                    
                    // Kiểm tra null trước khi sử dụng
                    if (emergency.Username == null || emergency.HospitalId == null)
                    {
                        continue; // Bỏ qua nếu thiếu thông tin cần thiết
                    }
                    
                    if (user != null && hospital != null)
                    {
                        // Tính khoảng cách dựa trên địa chỉ bệnh viện so với điểm mốc
                        double distance = CalculateDistanceFromHospitalAddress(hospital.HospitalAddress ?? "");
                        
                        if (distance <= request.Radius)
                        {
                            double distanceRaw = Math.Round(distance, 2);
                            string formattedDistance = FormatDistance(distanceRaw);
                            
                            mockRequests.Add(new BloodRequestResult
                            {
                                Id = emergency.EmergencyId.ToString(),
                                Distance = formattedDistance, // Only use formatted distance
                                BloodType = emergency.BloodType ?? "Unknown",
                                Status = emergency.EmergencyStatus ?? "Unknown",
                                Location = new Location
                                {
                                    Latitude = 0, // Không có tọa độ thực tế
                                    Longitude = 0,
                                    Address = hospital.HospitalAddress ?? "Unknown"
                                },
                                RequesterInfo = new RequesterInfo
                                {
                                    Name = user.FullName ?? "Anonymous",
                                    Phone = user.Phone ?? "No phone provided",
                                    Email = user.Email ?? "No email provided"
                                }
                            });
                        }
                    }
                }
                
                // Sắp xếp danh sách yêu cầu máu theo khoảng cách từ thấp đến cao
                response.Requests = SortRequestsByDistance(mockRequests);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error searching for blood requests: {ex.Message}");
                // You might want to log the error
            }
            
            return response;
        }

        /// <summary>
        /// Tính khoảng cách dựa trên địa chỉ bệnh viện so với điểm mốc: 7 Đ. D1, Long Thạnh Mỹ, Thủ Đức
        /// </summary>
        private double CalculateDistanceFromHospitalAddress(string hospitalAddress)
        {
            hospitalAddress = hospitalAddress ?? string.Empty;
            
            if (string.IsNullOrEmpty(hospitalAddress))
                return double.MaxValue;
            
            var random = new Random();
            
            // Thủ Đức (nơi có điểm mốc - 7 Đ. D1, Long Thạnh Mỹ)
            if (hospitalAddress.Contains("Long Thạnh Mỹ") || 
                (hospitalAddress.Contains("Thủ Đức") && hospitalAddress.Contains("D1")))
            {
                return 0.5; // Rất gần điểm mốc
            }
            
            // Khu vực Thủ Đức
            if (hospitalAddress.Contains("Thủ Đức") || 
                hospitalAddress.Contains("Linh Trung") ||
                hospitalAddress.Contains("Linh Tây") ||
                hospitalAddress.Contains("Linh Đông") ||
                hospitalAddress.Contains("Hiệp Phú"))
            {
                return random.NextDouble() * 2 + 2; // 2-4km
            }
            
            // Các quận lân cận Thủ Đức
            if (hospitalAddress.Contains("Quận 9") || hospitalAddress.Contains("Q9") ||
                hospitalAddress.Contains("Quận 2") || hospitalAddress.Contains("Q2") ||
                hospitalAddress.Contains("Bình Thạnh"))
            {
                return random.NextDouble() * 3 + 5; // 5-8km
            }
            
            // Các quận nội thành TP.HCM
            if (hospitalAddress.Contains("Quận 1") || hospitalAddress.Contains("Q1") ||
                hospitalAddress.Contains("Quận 3") || hospitalAddress.Contains("Q3") ||
                hospitalAddress.Contains("Quận 5") || hospitalAddress.Contains("Q5") ||
                hospitalAddress.Contains("Quận 7") || hospitalAddress.Contains("Q7") ||
                hospitalAddress.Contains("Quận 10") || hospitalAddress.Contains("Q10") ||
                hospitalAddress.Contains("Tân Bình") ||
                hospitalAddress.Contains("Phú Nhuận"))
            {
                return random.NextDouble() * 5 + 8; // 8-13km
            }
            
            // Các quận khác trong TP.HCM
            if (hospitalAddress.Contains("TP.HCM") || hospitalAddress.Contains("HCM") || 
                hospitalAddress.Contains("Hồ Chí Minh") ||
                hospitalAddress.Contains("Quận 4") || hospitalAddress.Contains("Q4") ||
                hospitalAddress.Contains("Quận 6") || hospitalAddress.Contains("Q6") ||
                hospitalAddress.Contains("Quận 8") || hospitalAddress.Contains("Q8") ||
                hospitalAddress.Contains("Quận 11") || hospitalAddress.Contains("Q11") ||
                hospitalAddress.Contains("Quận 12") || hospitalAddress.Contains("Q12"))
            {
                return random.NextDouble() * 8 + 10; // 10-18km
            }
            
            // Các tỉnh lân cận TP.HCM
            if (hospitalAddress.Contains("Bình Dương") ||
                hospitalAddress.Contains("Đồng Nai") ||
                hospitalAddress.Contains("Long An") ||
                hospitalAddress.Contains("Tây Ninh") ||
                hospitalAddress.Contains("Vũng Tàu"))
            {
                return random.NextDouble() * 30 + 20; // 20-50km
            }
            
            // Các tỉnh miền Tây Nam Bộ
            if (hospitalAddress.Contains("Tiền Giang") ||
                hospitalAddress.Contains("Bến Tre") ||
                hospitalAddress.Contains("Vĩnh Long") ||
                hospitalAddress.Contains("Cần Thơ") ||
                hospitalAddress.Contains("An Giang") ||
                hospitalAddress.Contains("Hậu Giang") ||
                hospitalAddress.Contains("Kiên Giang") ||
                hospitalAddress.Contains("Đồng Tháp") ||
                hospitalAddress.Contains("Cà Mau"))
            {
                return random.NextDouble() * 100 + 50; // 50-150km
            }
            
            // Các tỉnh miền Trung
            if (hospitalAddress.Contains("Đà Nẵng") ||
                hospitalAddress.Contains("Huế") || hospitalAddress.Contains("Thừa Thiên") ||
                hospitalAddress.Contains("Quảng Nam") ||
                hospitalAddress.Contains("Quảng Ngãi") ||
                hospitalAddress.Contains("Bình Định") ||
                hospitalAddress.Contains("Khánh Hòa") || hospitalAddress.Contains("Nha Trang") ||
                hospitalAddress.Contains("Nghệ An") ||
                hospitalAddress.Contains("Hà Tĩnh"))
            {
                return random.NextDouble() * 200 + 500; // 500-700km
            }
            
            // Các tỉnh miền Bắc
            if (hospitalAddress.Contains("Hà Nội") ||
                hospitalAddress.Contains("Hải Phòng") ||
                hospitalAddress.Contains("Quảng Ninh") ||
                hospitalAddress.Contains("Bắc Ninh") ||
                hospitalAddress.Contains("Hưng Yên") ||
                hospitalAddress.Contains("Hải Dương") ||
                hospitalAddress.Contains("Nam Định"))
            {
                return random.NextDouble() * 200 + 1000; // 1000-1200km
            }
            
            // Các tỉnh khác
            return random.NextDouble() * 500 + 100; // 100-600km
        }

        // Haversine formula to calculate distance between two points on Earth
        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double EarthRadiusKm = 6371;
            
            var dLat = DegreesToRadians(lat2 - lat1);
            var dLon = DegreesToRadians(lon2 - lon1);
            
            lat1 = DegreesToRadians(lat1);
            lat2 = DegreesToRadians(lat2);
            
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(lat1) * Math.Cos(lat2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            
            return EarthRadiusKm * c;
        }
        
        private double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }
        
        /// <summary>
        /// Format distance in appropriate units (m or km)
        /// </summary>
        /// <param name="distanceInKm">Distance in kilometers</param>
        /// <returns>Formatted distance string with units</returns>
        private string FormatDistance(double distanceInKm)
        {
            if (distanceInKm < 1)
            {
                // Convert to meters if less than 1 km
                int meters = (int)(distanceInKm * 1000);
                return $"{meters} m";
            }
            else if (distanceInKm < 10)
            {
                // For distances less than 10km, show one decimal place
                return $"{distanceInKm:F1} km";
            }
            else
            {
                // For larger distances, round to integers
                return $"{Math.Round(distanceInKm)} km";
            }
        }
          /// <summary>
        /// Sắp xếp danh sách yêu cầu máu theo khoảng cách từ thấp đến cao
        /// </summary>
        private List<BloodRequestResult> SortRequestsByDistance(List<BloodRequestResult> requests)
        {
            // Since Distance is now a string, we need to use a numeric value for sorting
            var requestsWithNumericDistance = new List<(BloodRequestResult Request, double Distance)>();
            
            foreach (var bloodRequest in requests)
            {
                // Trích xuất giá trị khoảng cách số từ chuỗi định dạng (như "800 m" hoặc "5.2 km")
                double numericDistance = ExtractNumericDistance(bloodRequest.Distance);
                requestsWithNumericDistance.Add((bloodRequest, numericDistance));
            }
            
            // Sắp xếp theo khoảng cách từ thấp đến cao (gần đến xa)
            var sortedRequests = requestsWithNumericDistance
                .OrderBy(r => r.Distance)
                .Select(r => r.Request)
                .ToList();
                
            Console.WriteLine($"Sorted {sortedRequests.Count} blood requests by distance (from lowest to highest)");
            return sortedRequests;
        }
        
        /// <summary>
        /// Trích xuất giá trị số từ chuỗi khoảng cách được định dạng
        /// </summary>
        /// <param name="formattedDistance">Chuỗi khoảng cách định dạng (ví dụ: "800 m" hoặc "5.2 km")</param>
        /// <returns>Giá trị khoảng cách tính bằng km</returns>
        private double ExtractNumericDistance(string formattedDistance)
        {
            if (string.IsNullOrEmpty(formattedDistance))
                return double.MaxValue; // Giá trị lớn nhất cho các trường hợp không có khoảng cách
                
            try
            {
                // Cắt chuỗi để lấy phần số và đơn vị
                string[] parts = formattedDistance.Split(' ');
                if (parts.Length != 2)
                    return double.MaxValue;
                
                // Lấy giá trị số
                if (!double.TryParse(parts[0], out double value))
                    return double.MaxValue;
                
                // Kiểm tra đơn vị và chuyển đổi về km nếu cần
                string unit = parts[1].ToLower();
                if (unit == "m")
                    return value / 1000.0; // Chuyển từ m sang km
                else if (unit == "km")
                    return value;
                
                return double.MaxValue;
            }
            catch
            {
                return double.MaxValue;
            }
        }
    }
}