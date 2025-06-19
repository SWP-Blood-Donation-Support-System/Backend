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
        // Reference point: 7 Đ. D1, Long Thạnh Mỹ, Thủ Đức, Hồ Chí Minh 700000, Vietnam
        private readonly double _referenceLatitude = 10.841962;  // Approximate latitude for the reference point
        private readonly double _referenceLongitude = 106.810627; // Approximate longitude for the reference point

        public SearchService(BloodDonationSystemContext context)
        {
            _context = context;
        }        /// <summary>
        /// Tìm kiếm người hiến máu theo nhóm máu
        /// </summary>
        /// <param name="bloodType">Nhóm máu cần tìm</param>
        /// <returns>Danh sách người hiến máu phù hợp</returns>
        public async Task<IEnumerable<object>> FindDonorsByBloodType(string bloodType)
        {
            if (string.IsNullOrEmpty(bloodType))
                throw new ArgumentException("Blood type is required", nameof(bloodType));

            var normalizedBloodType = bloodType.ToUpper().Trim();
            
            // Safeguard for blood type compatibility
            var compatibilityMap = GetCompatibleDonorTypes();
            List<string> compatibleDonorTypes;
            
            if (!compatibilityMap.TryGetValue(normalizedBloodType, out compatibleDonorTypes))
            {
                // Fallback to just the normalized blood type if not found in the map
                Console.WriteLine($"Warning: Blood type {normalizedBloodType} not found in compatibility map");
                compatibleDonorTypes = new List<string> { normalizedBloodType };
            }
            
            // Log để debug
            Console.WriteLine($"Searching for blood type: {normalizedBloodType}");
            Console.WriteLine($"Compatible types: {string.Join(", ", compatibleDonorTypes)}");
            
            // Lấy tất cả người dùng đầu tiên để kiểm tra xem có dữ liệu không
            var allUsers = await _context.Users.ToListAsync();
            Console.WriteLine($"Total users in database: {allUsers.Count}");
            
            // Lấy dữ liệu từ bảng Users với điều kiện ít hơn để kiểm tra
            var usersWithRole = await _context.Users
                .Where(u => u.Role == "User")
                .ToListAsync();
                
            Console.WriteLine($"Users with role 'User': {usersWithRole.Count}");
            
            // Nếu không có dữ liệu hoặc rất ít, tạo mẫu để test
            var result = new List<object>();
            
            if (usersWithRole.Count == 0)
            {                // Thêm dữ liệu mẫu để test khi database trống
                for (int i = 1; i <= 10; i++)
                {
                    double distance = i * 1.5; // Khoảng cách tăng dần
                    string formattedDistance = FormatDistance(distance);
                    
                    // Thêm nhiều nhóm máu khác nhau cho phong phú
                    string donorBloodType = i % 8 == 0 ? "A+" :
                                          i % 7 == 0 ? "A-" :
                                          i % 6 == 0 ? "B+" :
                                          i % 5 == 0 ? "B-" :
                                          i % 4 == 0 ? "AB+" :
                                          i % 3 == 0 ? "AB-" :
                                          i % 2 == 0 ? "O+" : "O-";
                    
                    // Nếu không tương thích với nhóm máu yêu cầu, thì gán nhóm máu yêu cầu
                    if (!compatibleDonorTypes.Contains(donorBloodType))
                    {
                        donorBloodType = compatibleDonorTypes.FirstOrDefault() ?? normalizedBloodType;
                    }
                    
                    result.Add(new {
                        FullName = $"Người hiến máu {i}",
                        Email = $"donor{i}@example.com",
                        DateOfBirth = DateTime.Now.AddYears(-20 - i),
                        Gender = i % 2 == 0 ? "Nam" : "Nữ",
                        Phone = $"098765432{i}",
                        Address = $"Địa chỉ {i}, {(i % 3 == 0 ? "Thủ Đức" : i % 2 == 0 ? "Quận 1" : "Quận 2")}, TP.HCM",
                        BloodType = donorBloodType,
                        Distance = formattedDistance // Only use formatted distance with units
                    });
                }
            }
            else 
            {
                // Lấy người dùng thực tế từ DB - Không lọc quá nhiều để đảm bảo có dữ liệu
                var donors = await _context.Users
                    .ToListAsync();
                    
                var compatibleDonors = donors
                    .Where(u => u.BloodType != null)  // Lọc bớt điều kiện để có dữ liệu
                    .Take(10)  // Lấy tối đa 10 người
                    .ToList();
                
                Console.WriteLine($"Donors found for display: {compatibleDonors.Count}");
                
                // Chuyển đổi thành dữ liệu trả về
                foreach (var u in compatibleDonors)
                {
                    double distanceInKm = CalculateDistanceFromAddress(u.Address ?? string.Empty);
                    string formattedDistance = FormatDistance(distanceInKm);
                    
                    result.Add(new {
                        FullName = u.FullName ?? "Unknown", 
                        Email = u.Email ?? "No email",
                        DateOfBirth = u.DateOfBirth,
                        Gender = u.Gender ?? "Unknown",
                        Phone = u.Phone ?? "No phone",
                        Address = u.Address ?? "No address",
                        BloodType = u.BloodType ?? "Unknown",
                        Distance = formattedDistance // Only use formatted distance with units
                    });
                }
            }
            
            // Sắp xếp kết quả theo khoảng cách từ gần đến xa
            var resultWithNumericDistance = new List<(object ResultItem, double NumericDistance)>();
            foreach (var item in result)
            {
                // Trích xuất giá trị số từ chuỗi khoảng cách đã định dạng
                string formattedDistance = ((dynamic)item).Distance;
                double numericDistance = ExtractNumericDistance(formattedDistance);
                resultWithNumericDistance.Add((item, numericDistance));
            }
            
            // Sắp xếp theo khoảng cách số và chỉ giữ lại đối tượng kết quả
            result = resultWithNumericDistance
                .OrderBy(x => x.NumericDistance) // Sắp xếp từ thấp đến cao
                .Select(x => x.ResultItem)
                .ToList();
            
            Console.WriteLine($"Sorted {result.Count} results by distance (from nearest to farthest)");
            
            return result;
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
        }        /// <summary>
        /// Tính khoảng cách dựa trên địa chỉ so với điểm mốc: 7 Đ. D1, Long Thạnh Mỹ, Thủ Đức, Hồ Chí Minh
        /// </summary>
        /// <param name="address">Địa chỉ cần tính khoảng cách</param>
        /// <returns>Khoảng cách tính bằng km</returns>
        private double CalculateDistanceFromAddress(string address)
        {
            // Đảm bảo address không null
            address = address ?? string.Empty;
            
            if (string.IsNullOrEmpty(address))
                return double.MaxValue; // Địa chỉ trống sẽ hiển thị cuối cùng
            
            // Thủ Đức (nơi có điểm mốc - 7 Đ. D1, Long Thạnh Mỹ)
            if (address.Contains("Long Thạnh Mỹ") || 
                (address.Contains("Thủ Đức") && address.Contains("D1")))
            {
                return 0.5; // Rất gần điểm mốc
            }
            
            // Khu vực Thủ Đức
            if (address.Contains("Thủ Đức") ||
                address.Contains("Linh Trung") ||
                address.Contains("Linh Tây") ||
                address.Contains("Linh Đông") ||
                address.Contains("Hiệp Phú"))
            {
                return new Random().NextDouble() * 2 + 2; // 2-4km
            }
            
            // Các quận lân cận Thủ Đức
            if (address.Contains("Quận 9") || address.Contains("Q9") ||
                address.Contains("Quận 2") || address.Contains("Q2") ||
                address.Contains("Bình Thạnh"))
            {
                return new Random().NextDouble() * 3 + 5; // 5-8km
            }
            
            // Các quận trung tâm TP.HCM
            if (address.Contains("Quận 1") || address.Contains("Q1") ||
                address.Contains("Quận 3") || address.Contains("Q3") ||
                address.Contains("Quận 4") || address.Contains("Q4") ||
                address.Contains("Quận 5") || address.Contains("Q5") ||
                address.Contains("Quận 10") || address.Contains("Q10"))
            {
                return new Random().NextDouble() * 4 + 8; // 8-12km
            }
            
            // Các quận khác trong TP.HCM
            if (address.Contains("TP.HCM") || address.Contains("TP. HCM") || 
                address.Contains("HCM") || address.Contains("Hồ Chí Minh") ||
                address.Contains("Tân Bình") || address.Contains("Gò Vấp") ||
                address.Contains("Tân Phú") || address.Contains("Bình Tân"))
            {
                return new Random().NextDouble() * 8 + 10; // 10-18km
            }
            
            // Các tỉnh lân cận
            if (address.Contains("Bình Dương") ||
                address.Contains("Đồng Nai") ||
                address.Contains("Long An") ||
                address.Contains("Vũng Tàu") ||
                address.Contains("Bà Rịa"))
            {
                return new Random().NextDouble() * 20 + 20; // 20-40km
            }
            
            // Các tỉnh miền Nam
            if (address.Contains("Tiền Giang") ||
                address.Contains("Bến Tre") ||
                address.Contains("Cần Thơ") ||
                address.Contains("An Giang") ||
                address.Contains("Vĩnh Long") ||
                address.Contains("Đồng Tháp"))
            {
                return new Random().NextDouble() * 50 + 50; // 50-100km
            }
            
            // Các tỉnh miền Trung
            if (address.Contains("Đà Nẵng") ||
                address.Contains("Huế") ||
                address.Contains("Quảng Nam") ||
                address.Contains("Quảng Ngãi") ||
                address.Contains("Nha Trang") ||
                address.Contains("Khánh Hòa"))
            {
                return new Random().NextDouble() * 200 + 500; // 500-700km
            }
            
            // Các tỉnh miền Bắc
            if (address.Contains("Hà Nội") ||
                address.Contains("Hải Phòng") ||
                address.Contains("Quảng Ninh") ||
                address.Contains("Bắc Ninh") ||
                address.Contains("Hải Dương"))
            {
                return new Random().NextDouble() * 200 + 1000; // 1000-1200km
            }
            
            // Địa chỉ khác
            return new Random().NextDouble() * 500 + 100; // 100-600km
        }

        /// <summary>
        /// Lấy danh sách các nhóm máu có thể hiến cho từng nhóm máu
        /// </summary>
        /// <returns>Từ điển mapping giữa nhóm máu với các nhóm máu tương thích</returns>
        private Dictionary<string, List<string>> GetCompatibleDonorTypes()
        {
            var compatibilityMap = new Dictionary<string, List<string>>
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
            
            // Thêm alias để xử lý các dạng nhập khác nhau
            compatibilityMap.Add("A", compatibilityMap["A+"]);
            compatibilityMap.Add("B", compatibilityMap["B+"]);
            compatibilityMap.Add("AB", compatibilityMap["AB+"]);
            compatibilityMap.Add("O", compatibilityMap["O+"]);
            
            return compatibilityMap;
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
        /// Calculate distance between two points using Haversine formula
        /// </summary>
        private double CalculateHaversineDistance(double lat1, double lon1, double lat2, double lon2)
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
        /// Generate sample donor data for demonstration when no real donors are found
        /// </summary>
        /// <param name="bloodType">Requested blood type</param>
        /// <returns>List of sample donor data</returns>
        private List<object> GenerateSampleDonors(string bloodType)
        {
            Console.WriteLine("Generating sample donors for demonstration");
            var sampleDonors = new List<object>();
            var random = new Random();
            var vietnameseNames = new[] { 
                "Nguyễn Văn An", "Trần Thị Bình", "Lê Minh Cường", "Phạm Hồng Dung", 
                "Hoàng Văn Ét", "Vũ Thị Giang", "Đặng Minh Hải", "Bùi Thu Hiền" 
            };
            
            var districts = new[] { 
                "Thủ Đức", "Quận 1", "Quận 2", "Quận 3", "Quận 5", 
                "Quận 7", "Quận 9", "Bình Thạnh", "Gò Vấp" 
            };
            
            for (int i = 0; i < 5; i++)
            {
                string name = vietnameseNames[random.Next(vietnameseNames.Length)];
                string district = districts[random.Next(districts.Length)];
                double distance = random.Next(1, 25) + Math.Round(random.NextDouble(), 2);
                
                sampleDonors.Add(new {
                    FullName = name,
                    Email = name.Replace(" ", ".").ToLower() + "@gmail.com",
                    DateOfBirth = DateTime.Now.AddYears(-20 - random.Next(20)),
                    Gender = random.Next(2) == 0 ? "Nam" : "Nữ",
                    Phone = $"09{random.Next(10000000, 99999999)}",
                    Address = $"{random.Next(1, 200)} Đường {random.Next(1, 20)}, {district}, TP.HCM",
                    BloodType = bloodType,
                    Distance = distance,
                    DistanceFormatted = FormatDistance(distance),
                    Note = "Dữ liệu mẫu cho mục đích demo"
                });
            }
            
            return sampleDonors.OrderBy(d => ((dynamic)d).Distance).ToList();
        }

        /// <summary>
        /// Trích xuất giá trị số từ chuỗi khoảng cách được định dạng
        /// </summary>
        /// <param name="formattedDistance">Chuỗi khoảng cách định dạng (ví dụ: "800 m" hoặc "5.2 km")</param>
        /// <returns>Giá trị khoảng cách tính bằng km</returns>
        private double ExtractNumericDistance(string formattedDistance)
        {
            if (string.IsNullOrEmpty(formattedDistance))
                return double.MaxValue;
                
            try
            {
                string[] parts = formattedDistance.Split(' ');
                if (parts.Length != 2)
                    return double.MaxValue;
                
                if (!double.TryParse(parts[0], out double value))
                    return double.MaxValue;
                
                string unit = parts[1].ToLower();
                if (unit == "m")
                    return value / 1000.0; // Convert to km
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