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
            var response = new BloodRequestSearchResponse();
            
            try
            {
                // If the database doesn't have BloodRequests yet, create temp data for testing
                // In a real application, you'd remove this and use only the actual database data
                var mockRequests = new List<BloodRequestResult>();
                
                // Get users who are near the specified location
                var users = await _context.Users.ToListAsync();
                
                // Use users as mock blood requesters for now
                foreach (var user in users.Where(u => !string.IsNullOrEmpty(u.Address) && !string.IsNullOrEmpty(u.BloodType)))
                {
                    // Generate mock coordinates based on address (in real app, you'd use geocoding)
                    var mockLat = request.Lat + (new Random().NextDouble() - 0.5) * 0.1;
                    var mockLng = request.Lng + (new Random().NextDouble() - 0.5) * 0.1;
                    
                    double distance = CalculateDistance(request.Lat, request.Lng, mockLat, mockLng);
                    
                    // Only include if within radius and matches blood type (if specified)
                    if (distance <= request.Radius && 
                        (string.IsNullOrEmpty(request.BloodType) || user.BloodType == request.BloodType))
                    {                        mockRequests.Add(new BloodRequestResult
                        {
                            Id = user.Username,
                            Distance = Math.Round(distance, 2),
                            BloodType = user.BloodType ?? "Unknown",
                            Status = "PENDING",
                            Location = new Location
                            {
                                Latitude = mockLat,
                                Longitude = mockLng,
                                Address = user.Address ?? "No address provided"
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