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
    // Note: This service is no longer needed since we've removed the nearby search functionality
    public class BloodRequestSearchService : IBloodRequestSearchService
    {
        private readonly BloodDonationSystemContext _context;

        public BloodRequestSearchService(BloodDonationSystemContext context)
        {
            _context = context;
        }

        public async Task<BloodRequestSearchResponse> FindNearbyBloodRequests(BloodRequestSearchRequest request)
        {
            // Validate the request parameters
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var response = new BloodRequestSearchResponse();
            
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
                        // Tính khoảng cách dựa trên địa chỉ bệnh viện
                        double distance = CalculateDistance(request.Lat, request.Lng, 0, 0); // Không có tọa độ thực tế
                        
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