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
            {                // Lấy các yêu cầu máu từ bảng Emergency
                var emergencies = await _context.Emergencies
                    .Where(e => string.IsNullOrEmpty(request.BloodType) || 
                               (e.BloodType != null && e.BloodType == request.BloodType))
                    .ToListAsync();
                
                var mockRequests = new List<BloodRequestResult>();
                
                foreach (var emergency in emergencies)
                {                    // Lấy thông tin người dùng
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
                        {                            mockRequests.Add(new BloodRequestResult
                            {
                                Id = emergency.EmergencyId.ToString(),
                                Distance = Math.Round(distance, 2),
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
                
                response.Requests = mockRequests.OrderBy(r => r.Distance).ToList();
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
    }
}