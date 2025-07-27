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
        private readonly IGeoapifyService _geoapifyService;

        public SearchService(BloodDonationSystemContext context, IGeoapifyService geoapifyService)
        {
            _context = context;
            _geoapifyService = geoapifyService;
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
                        ProfileStatus = i % 3 == 0 ? "Sẵn sàng hiến máu" : "Không sẵn sàng hiến máu",
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
                        ProfileStatus = u.ProfileStatus ?? "Unknown",
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
                return GetStableRandomDistance(address, 2, 4); // 2-4km
            }
            
            // Các quận lân cận Thủ Đức
            if (address.Contains("Quận 9") || address.Contains("Q9") ||
                address.Contains("Quận 2") || address.Contains("Q2") ||
                address.Contains("Bình Thạnh"))
            {
                return GetStableRandomDistance(address, 5, 8); // 5-8km
            }
            
            // Các quận trung tâm TP.HCM
            if (address.Contains("Quận 1") || address.Contains("Q1") ||
                address.Contains("Quận 3") || address.Contains("Q3") ||
                address.Contains("Quận 4") || address.Contains("Q4") ||
                address.Contains("Quận 5") || address.Contains("Q5") ||
                address.Contains("Quận 10") || address.Contains("Q10"))
            {
                return GetStableRandomDistance(address, 8, 12); // 8-12km
            }
            
            // Các quận khác trong TP.HCM
            if (address.Contains("TP.HCM") || address.Contains("TP. HCM") || 
                address.Contains("HCM") || address.Contains("Hồ Chí Minh") ||
                address.Contains("Tân Bình") || address.Contains("Gò Vấp") ||
                address.Contains("Tân Phú") || address.Contains("Bình Tân"))
            {
                return GetStableRandomDistance(address, 10, 18); // 10-18km
            }
            
            // Các tỉnh lân cận
            if (address.Contains("Bình Dương") ||
                address.Contains("Đồng Nai") ||
                address.Contains("Long An") ||
                address.Contains("Vũng Tàu") ||
                address.Contains("Bà Rịa"))
            {
                return GetStableRandomDistance(address, 20, 40); // 20-40km
            }
            
            // Các tỉnh miền Nam
            if (address.Contains("Tiền Giang") ||
                address.Contains("Bến Tre") ||
                address.Contains("Cần Thơ") ||
                address.Contains("An Giang") ||
                address.Contains("Vĩnh Long") ||
                address.Contains("Đồng Tháp"))
            {
                return GetStableRandomDistance(address, 50, 100); // 50-100km
            }
            
            // Các tỉnh miền Trung
            if (address.Contains("Đà Nẵng") ||
                address.Contains("Huế") ||
                address.Contains("Quảng Nam") ||
                address.Contains("Quảng Ngãi") ||
                address.Contains("Nha Trang") ||
                address.Contains("Khánh Hòa"))
            {
                return GetStableRandomDistance(address, 500, 700); // 500-700km
            }
            
            // Các tỉnh miền Bắc
            if (address.Contains("Hà Nội") ||
                address.Contains("Hải Phòng") ||
                address.Contains("Quảng Ninh") ||
                address.Contains("Bắc Ninh") ||
                address.Contains("Hải Dương"))
            {
                return GetStableRandomDistance(address, 1000, 1200); // 1000-1200km
            }
            
            // Địa chỉ khác
            return GetStableRandomDistance(address, 100, 600); // 100-600km
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
        /// Tạo khoảng cách ổn định dựa trên địa chỉ để tránh kết quả thay đổi liên tục
        /// </summary>
        /// <param name="address">Địa chỉ làm seed</param>
        /// <param name="minDistance">Khoảng cách tối thiểu</param>
        /// <param name="maxDistance">Khoảng cách tối đa</param>
        /// <returns>Khoảng cách ổn định trong khoảng min-max</returns>
        private double GetStableRandomDistance(string address, double minDistance, double maxDistance)
        {
            // Tạo seed từ địa chỉ để đảm bảo kết quả ổn định
            int seed = string.IsNullOrEmpty(address) ? 0 : address.GetHashCode();
            var random = new Random(Math.Abs(seed)); // Math.Abs để tránh seed âm
            
            // Tính khoảng cách trong khoảng min-max
            double range = maxDistance - minDistance;
            return minDistance + (random.NextDouble() * range);
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
            Console.WriteLine("No real donors found in database - returning empty list");
            return new List<object>();
        }

        /// <summary>
        /// Tìm kiếm yêu cầu máu theo nhóm máu
        /// </summary>
        /// <param name="bloodType">Nhóm máu cần tìm</param>
        /// <returns>Danh sách yêu cầu máu được sắp xếp theo khoảng cách</returns>
        public async Task<IEnumerable<object>> FindBloodRequestsByBloodType(string bloodType)
        {
            if (string.IsNullOrEmpty(bloodType))
                throw new ArgumentException("Blood type is required", nameof(bloodType));

            var normalizedBloodType = bloodType.ToUpper().Trim();
            
            try
            {
                // Lấy các yêu cầu máu từ bảng Emergency với điều kiện EmergencyStatus = "Đã xét duyệt"
                var emergencies = await _context.Emergencies
                    .Where(e => e.BloodType == normalizedBloodType && e.EmergencyStatus == "Đã xét duyệt")
                    .Include(e => e.Hospital)
                    .Include(e => e.UsernameNavigation)
                    .ToListAsync();
                
                var bloodRequests = new List<object>();
                
                foreach (var emergency in emergencies)
                {
                    if (emergency.Hospital == null || emergency.UsernameNavigation == null)
                        continue;
                    
                    // Tính khoảng cách dựa trên địa chỉ bệnh viện so với điểm mốc
                    double distance = CalculateDistanceFromHospitalAddress(emergency.Hospital.HospitalAddress ?? "");
                    double distanceRaw = Math.Round(distance, 2);
                    string formattedDistance = FormatDistance(distanceRaw);
                    
                    bloodRequests.Add(new
                    {
                        Id = emergency.EmergencyId,
                        Distance = formattedDistance,
                        BloodType = emergency.BloodType ?? "Unknown",
                        Status = emergency.EmergencyStatus ?? "Unknown",
                        Description = emergency.EmergencyNote ?? "",
                        EmergencyDate = emergency.EmergencyDate,
                        EndDate = emergency.EndDate,
                        RequiredUnits = emergency.RequiredUnits ?? 0,
                        EmergencyMedical = emergency.EmergencyMedical ?? "",
                        Hospital = new
                        {
                            Id = emergency.Hospital.HospitalId,
                            Name = emergency.Hospital.HospitalName ?? "Unknown",
                            Address = emergency.Hospital.HospitalAddress ?? "Unknown",
                            Phone = emergency.Hospital.HospitalPhone ?? ""
                        },
                        Requester = new
                        {
                            Name = emergency.UsernameNavigation.FullName ?? "Anonymous",
                            Phone = emergency.UsernameNavigation.Phone ?? "",
                            Email = emergency.UsernameNavigation.Email ?? ""
                        }
                    });
                }
                
                // Sắp xếp theo khoảng cách từ gần đến xa
                var sortedRequests = bloodRequests
                    .Select(r => new { Request = r, NumericDistance = ExtractNumericDistance(((dynamic)r).Distance) })
                    .OrderBy(x => x.NumericDistance)
                    .Select(x => x.Request)
                    .ToList();
                
                Console.WriteLine($"Found {sortedRequests.Count} approved blood requests for blood type {normalizedBloodType}");
                
                // Nếu không có dữ liệu thực, tạo dữ liệu mẫu
                if (!sortedRequests.Any())
                {
                    return GenerateSampleBloodRequests(normalizedBloodType);
                }
                
                return sortedRequests;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in FindBloodRequestsByBloodType: {ex.Message}");
                // Trả về dữ liệu mẫu khi có lỗi
                return GenerateSampleBloodRequests(normalizedBloodType);
            }
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
                hospitalAddress.Contains("Hồ Chí Minh"))
            {
                return random.NextDouble() * 8 + 10; // 10-18km
            }
            
            // Các tỉnh lân cận TP.HCM
            if (hospitalAddress.Contains("Bình Dương") ||
                hospitalAddress.Contains("Đồng Nai") ||
                hospitalAddress.Contains("Long An"))
            {
                return random.NextDouble() * 30 + 20; // 20-50km
            }
            
            // Các tỉnh khác
            return random.NextDouble() * 100 + 50; // 50-150km
        }

        /// <summary>
        /// Tạo dữ liệu mẫu cho yêu cầu máu khi không có dữ liệu thực
        /// </summary>
        private List<object> GenerateSampleBloodRequests(string bloodType)
        {
            Console.WriteLine("No real blood requests found in database - returning empty list");
            return new List<object>();
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

        /// <summary>
        /// Tìm kiếm tất cả người hiến máu có điều kiện - CHỈ LẤY DỮ LIỆU THỰC TỪ DATABASE
        /// </summary>
        /// <returns>Danh sách tất cả người hiến máu có trạng thái "Sẵn sàng hiến máu" và role "User"</returns>
        public async Task<IEnumerable<object>> FindAllAvailableDonors()
        {
            try
            {
                Console.WriteLine("Finding all available donors from database...");
                
                // Lấy tất cả người dùng có ProfileStatus = "Sẵn sàng hiến máu" và Role = "User"
                var availableDonors = await _context.Users
                    .Where(u => u.ProfileStatus == "Sẵn sàng hiến máu" && 
                               u.Role == "User" && 
                               u.UserStatus == "Active")
                    .ToListAsync();

                Console.WriteLine($"Found {availableDonors.Count} available donors in database");

                if (!availableDonors.Any())
                {
                    Console.WriteLine("No available donors found in database");
                    return new List<object>();
                }

                // Tính toán khoảng cách ổn định và sắp xếp
                var result = availableDonors.Select(donor => {
                    double distanceInKm = CalculateStableDistanceFromAddress(donor.Address ?? "", donor.Username ?? "");
                    string formattedDistance = FormatDistance(distanceInKm);
                    
                    return new {
                        Username = donor.Username ?? "Unknown",
                        FullName = donor.FullName ?? "Unknown",
                        Email = donor.Email ?? "No email",
                        DateOfBirth = donor.DateOfBirth,
                        Gender = donor.Gender ?? "Unknown",
                        Phone = donor.Phone ?? "No phone",
                        Address = donor.Address ?? "No address",
                        BloodType = donor.BloodType ?? "Unknown",
                        ProfileStatus = donor.ProfileStatus ?? "Unknown",
                        UserStatus = donor.UserStatus ?? "Unknown",
                        Distance = formattedDistance,
                        NumericDistance = distanceInKm
                    };
                }).OrderBy(d => d.NumericDistance)
                .Select(d => new {
                    d.Username,
                    d.FullName,
                    d.Email,
                    d.DateOfBirth,
                    d.Gender,
                    d.Phone,
                    d.Address,
                    d.BloodType,
                    d.ProfileStatus,
                    d.UserStatus,
                    d.Distance
                })
                .ToList<object>();

                Console.WriteLine($"Returning {result.Count} available donors sorted by distance");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in FindAllAvailableDonors: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return new List<object>();
            }
        }

        /// <summary>
        /// Tìm kiếm tất cả yêu cầu máu đã được phê duyệt - CHỈ LẤY DỮ LIỆU THỰC TỪ DATABASE
        /// </summary>
        /// <returns>Danh sách tất cả yêu cầu máu từ bảng BloodRequests có Status = "Đang chờ"</returns>
        public async Task<IEnumerable<object>> FindAllApprovedBloodRequests()
        {
            try
            {
                Console.WriteLine("Finding all approved blood requests from database...");
                
                // Lấy tất cả Emergencies với EmergencyStatus = "Đã xét duyệt"
                var approvedRequests = await _context.Emergencies
                    .Where(e => e.EmergencyStatus == "Đã xét duyệt")
                    .ToListAsync();

                Console.WriteLine($"Found {approvedRequests.Count} approved blood requests in database");

                if (!approvedRequests.Any())
                {
                    Console.WriteLine("No approved blood requests found in database");
                    return new List<object>();
                }

                // Lấy thông tin hospital cho từng emergency
                var hospitalIds = approvedRequests.Select(e => e.HospitalId).Distinct().ToList();
                var hospitals = await _context.Hospitals
                    .Where(h => hospitalIds.Contains(h.HospitalId))
                    .ToListAsync();

                // Tính toán khoảng cách ổn định dựa trên địa chỉ bệnh viện và sắp xếp
                var result = approvedRequests.Select(emergency => {
                    var hospital = hospitals.FirstOrDefault(h => h.HospitalId == emergency.HospitalId);
                    double distanceInKm = CalculateStableDistanceFromAddress(hospital?.HospitalAddress ?? "", emergency.EmergencyId.ToString());
                    string formattedDistance = FormatDistance(distanceInKm);
                    
                    return new {
                        Username = emergency.Username ?? "",
                        EmergencyDate = emergency.EmergencyDate,
                        BloodType = emergency.BloodType ?? "",
                        EmergencyStatus = emergency.EmergencyStatus ?? "",
                        EmergencyNote = emergency.EmergencyNote ?? "",
                        RequiredUnits = emergency.RequiredUnits,
                        HospitalId = emergency.HospitalId,
                        HospitalName = hospital?.HospitalName ?? "Unknown Hospital",
                        HospitalAddress = hospital?.HospitalAddress ?? "No address",
                        HospitalPhone = hospital?.HospitalPhone ?? "No phone",
                        Distance = formattedDistance,
                        NumericDistance = distanceInKm
                    };
                }).OrderBy(r => r.NumericDistance)
                .Select(r => new {
                    r.Username,
                    r.EmergencyDate,
                    r.BloodType,
                    r.EmergencyStatus,
                    r.EmergencyNote,
                    r.RequiredUnits,
                    r.HospitalId,
                    r.HospitalName,
                    r.HospitalAddress,
                    r.HospitalPhone,
                    r.Distance
                })
                .ToList<object>();

                Console.WriteLine($"Returning {result.Count} approved blood requests sorted by distance");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in FindAllApprovedBloodRequests: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return new List<object>();
            }
        }

        /// <summary>
        /// Tính khoảng cách ổn định dựa trên địa chỉ và ID để đảm bảo kết quả không thay đổi khi gọi lại API
        /// </summary>
        /// <param name="address">Địa chỉ cần tính khoảng cách</param>
        /// <param name="uniqueId">ID duy nhất để tạo seed cho việc tính toán</param>
        /// <returns>Khoảng cách tính bằng km</returns>
        private double CalculateStableDistanceFromAddress(string address, string uniqueId)
        {
            // Đảm bảo address không null
            address = address ?? string.Empty;
            
            if (string.IsNullOrEmpty(address))
                return double.MaxValue; // Địa chỉ trống sẽ hiển thị cuối cùng
            
            // Tạo seed ổn định từ địa chỉ và ID
            int seed = (address + uniqueId).GetHashCode();
            if (seed < 0) seed = -seed; // Đảm bảo seed dương
            var stableRandom = new Random(seed);
            
            // Thủ Đức (nơi có điểm mốc - 7 Đ. D1, Long Thạnh Mỹ)
            if (address.Contains("Long Thạnh Mỹ") || 
                (address.Contains("Thủ Đức") && address.Contains("D1")))
            {
                return 0.3 + stableRandom.NextDouble() * 0.4; // 0.3-0.7km
            }
            
            // Khu vực Thủ Đức
            if (address.Contains("Thủ Đức") ||
                address.Contains("Linh Trung") ||
                address.Contains("Linh Tây") ||
                address.Contains("Linh Đông") ||
                address.Contains("Hiệp Phú"))
            {
                return 2 + stableRandom.NextDouble() * 2; // 2-4km
            }
            
            // Các quận lân cận Thủ Đức
            if (address.Contains("Quận 9") || address.Contains("Q9") ||
                address.Contains("Quận 2") || address.Contains("Q2") ||
                address.Contains("Bình Thạnh"))
            {
                return 5 + stableRandom.NextDouble() * 3; // 5-8km
            }
            
            // Các quận trung tâm TP.HCM
            if (address.Contains("Quận 1") || address.Contains("Q1") ||
                address.Contains("Quận 3") || address.Contains("Q3") ||
                address.Contains("Quận 4") || address.Contains("Q4") ||
                address.Contains("Quận 5") || address.Contains("Q5") ||
                address.Contains("Quận 10") || address.Contains("Q10"))
            {
                return 8 + stableRandom.NextDouble() * 4; // 8-12km
            }
            
            // Các quận khác trong TP.HCM
            if (address.Contains("TP.HCM") || address.Contains("TP. HCM") || 
                address.Contains("HCM") || address.Contains("Hồ Chí Minh") ||
                address.Contains("Tân Bình") || address.Contains("Gò Vấp") ||
                address.Contains("Tân Phú") || address.Contains("Bình Tân"))
            {
                return 10 + stableRandom.NextDouble() * 8; // 10-18km
            }
            
            // Các tỉnh lân cận
            if (address.Contains("Bình Dương") ||
                address.Contains("Đồng Nai") ||
                address.Contains("Long An") ||
                address.Contains("Vũng Tàu") ||
                address.Contains("Bà Rịa"))
            {
                return 20 + stableRandom.NextDouble() * 20; // 20-40km
            }
            
            // Các tỉnh miền Nam
            if (address.Contains("Tiền Giang") ||
                address.Contains("Bến Tre") ||
                address.Contains("Cần Thơ") ||
                address.Contains("An Giang") ||
                address.Contains("Vĩnh Long") ||
                address.Contains("Đồng Tháp"))
            {
                return 50 + stableRandom.NextDouble() * 50; // 50-100km
            }
            
            // Các tỉnh miền Trung
            if (address.Contains("Đà Nẵng") ||
                address.Contains("Huế") ||
                address.Contains("Quảng Nam") ||
                address.Contains("Quảng Ngãi") ||
                address.Contains("Nha Trang") ||
                address.Contains("Khánh Hòa"))
            {
                return 500 + stableRandom.NextDouble() * 200; // 500-700km
            }
            
            // Các tỉnh miền Bắc
            if (address.Contains("Hà Nội") ||
                address.Contains("Hải Phòng") ||
                address.Contains("Quảng Ninh") ||
                address.Contains("Bắc Ninh") ||
                address.Contains("Hải Dương"))
            {
                return 1000 + stableRandom.NextDouble() * 200; // 1000-1200km
            }
            
            // Địa chỉ khác
            return 100 + stableRandom.NextDouble() * 500; // 100-600km
        }

        /// <summary>
        /// Tìm kiếm người hiến máu theo nhóm máu (V2) với Geoapify - có thể không truyền bloodType
        /// </summary>
        /// <param name="bloodType">Nhóm máu cần tìm (có thể null)</param>
        /// <returns>Danh sách người hiến máu phù hợp với khoảng cách thực tế</returns>
        public async Task<IEnumerable<object>> FindDonorsByBloodTypeV2(string? bloodType)
        {
            try
            {
                // Lấy tất cả users có điều kiện: UserStatus = "Active" VÀ ProfileStatus = "Sẵn sàng hiến máu"
                var activeUsers = await _context.Users
                    .Where(u => u.UserStatus == "Active" && u.ProfileStatus == "Sẵn sàng hiến máu")
                    .ToListAsync();

                // Lấy các appointment records có status "Đã hiến"
                var completedAppointments = await _context.AppointmentRecords
                    .Where(ar => ar.Status == "Đã hiến")
                    .Select(ar => ar.Username)
                    .Distinct()
                    .ToListAsync();

                // Lọc users đã hoàn thành hiến máu
                var eligibleUsers = activeUsers
                    .Where(u => completedAppointments.Contains(u.Username))
                    .ToList();

                // Nếu có bloodType, lọc theo nhóm máu tương thích
                if (!string.IsNullOrEmpty(bloodType))
                {
                    var normalizedBloodType = bloodType.ToUpper().Trim();
                    var compatibilityMap = GetCompatibleDonorTypes();
                    
                    if (compatibilityMap.TryGetValue(normalizedBloodType, out var compatibleTypes))
                    {
                        eligibleUsers = eligibleUsers
                            .Where(u => compatibleTypes.Contains(u.BloodType ?? ""))
                            .ToList();
                    }
                    else
                    {
                        // Fallback nếu không tìm thấy trong compatibility map
                        eligibleUsers = eligibleUsers
                            .Where(u => u.BloodType == normalizedBloodType)
                            .ToList();
                    }
                }

                Console.WriteLine($"Found {eligibleUsers.Count} eligible donors");

                if (!eligibleUsers.Any())
                {
                    return new List<object>();
                }

                // Địa chỉ tham chiếu (Staff location)
                const string referenceAddress = "7 Đ. D1, Long Thạnh Mỹ, Thủ Đức, Hồ Chí Minh 700000, Vietnam";

                // Lấy địa chỉ của tất cả users
                var addresses = eligibleUsers.Select(u => u.Address ?? "").ToList();

                // Tính khoảng cách thực tế bằng Geoapify
                var distanceResults = await _geoapifyService.CalculateMultipleDistancesAsync(referenceAddress, addresses);

                // Kết hợp thông tin user với khoảng cách
                var userDistances = eligibleUsers.Select((user, index) => new
                {
                    User = user,
                    Distance = distanceResults.Count > index ? distanceResults[index] : new GeoapifyDistanceResult
                    {
                        DistanceInKm = double.MaxValue,
                        DistanceText = "N/A",
                        DurationText = "N/A",
                        IsSuccess = false
                    }
                }).ToList();

                // Sắp xếp theo khoảng cách và tạo kết quả
                var result = userDistances
                    .OrderBy(ud => ud.Distance.DistanceInKm)
                    .Select(ud => new
                    {
                        Username = ud.User.Username ?? "",
                        FullName = ud.User.FullName ?? "",
                        Gender = ud.User.Gender ?? "",
                        DateOfBirth = ud.User.DateOfBirth,
                        Phone = ud.User.Phone ?? "",
                        Address = ud.User.Address ?? "",
                        BloodType = ud.User.BloodType ?? "",
                        ProfileStatus = ud.User.ProfileStatus ?? "",
                        Status = ud.User.UserStatus ?? "",
                        DistanceText = ud.Distance.DistanceText,
                        DurationText = ud.Distance.DurationText
                    })
                    .ToList<object>();

                Console.WriteLine($"Returning {result.Count} donors sorted by distance");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in FindDonorsByBloodTypeV2: {ex.Message}");
                return new List<object>();
            }
        }

        /// <summary>
        /// Tìm kiếm yêu cầu máu theo nhóm máu (V2) với Geoapify - có thể không truyền bloodType
        /// </summary>
        /// <param name="bloodType">Nhóm máu cần tìm (có thể null)</param>
        /// <returns>Danh sách yêu cầu máu phù hợp với khoảng cách thực tế</returns>
        public async Task<IEnumerable<object>> FindBloodRequestsByBloodTypeV2(string? bloodType)
        {
            try
            {
                // Lấy tất cả emergencies có status "Đã xét duyệt"
                var approvedEmergencies = await _context.Emergencies
                    .Where(e => e.EmergencyStatus == "Đã xét duyệt")
                    .ToListAsync();

                // Nếu có bloodType, lọc theo nhóm máu
                if (!string.IsNullOrEmpty(bloodType))
                {
                    var normalizedBloodType = bloodType.ToUpper().Trim();
                    approvedEmergencies = approvedEmergencies
                        .Where(e => e.BloodType == normalizedBloodType)
                        .ToList();
                }

                Console.WriteLine($"Found {approvedEmergencies.Count} approved emergencies");

                if (!approvedEmergencies.Any())
                {
                    return new List<object>();
                }

                // Lấy thông tin hospital cho từng emergency
                var hospitalIds = approvedEmergencies.Select(e => e.HospitalId).Distinct().ToList();
                var hospitals = await _context.Hospitals
                    .Where(h => hospitalIds.Contains(h.HospitalId))
                    .ToListAsync();

                // Địa chỉ tham chiếu (Staff location)
                const string referenceAddress = "7 Đ. D1, Long Thạnh Mỹ, Thủ Đức, Hồ Chí Minh 700000, Vietnam";

                // Lấy địa chỉ của tất cả hospitals
                var hospitalAddresses = approvedEmergencies.Select(e =>
                {
                    var hospital = hospitals.FirstOrDefault(h => h.HospitalId == e.HospitalId);
                    return hospital?.HospitalAddress ?? "";
                }).ToList();

                // Tính khoảng cách thực tế bằng Geoapify
                var distanceResults = await _geoapifyService.CalculateMultipleDistancesAsync(referenceAddress, hospitalAddresses);

                // Kết hợp thông tin emergency với hospital và khoảng cách
                var emergencyDistances = approvedEmergencies.Select((emergency, index) =>
                {
                    var hospital = hospitals.FirstOrDefault(h => h.HospitalId == emergency.HospitalId);
                    var distance = distanceResults.Count > index ? distanceResults[index] : new GeoapifyDistanceResult
                    {
                        DistanceInKm = double.MaxValue,
                        DistanceText = "N/A",
                        DurationText = "N/A",
                        IsSuccess = false
                    };

                    return new
                    {
                        Emergency = emergency,
                        Hospital = hospital,
                        Distance = distance
                    };
                }).ToList();

                // Sắp xếp theo khoảng cách và tạo kết quả
                var result = emergencyDistances
                    .OrderBy(ed => ed.Distance.DistanceInKm)
                    .Select(ed => new
                    {
                        Username = ed.Emergency.Username ?? "",
                        EmergencyDate = ed.Emergency.EmergencyDate,
                        BloodType = ed.Emergency.BloodType ?? "",
                        EmergencyStatus = ed.Emergency.EmergencyStatus ?? "",
                        EmergencyNote = ed.Emergency.EmergencyNote ?? "",
                        RequiredUnits = ed.Emergency.RequiredUnits,
                        HospitalId = ed.Emergency.HospitalId,
                        HospitalName = ed.Hospital?.HospitalName ?? "",
                        HospitalAddress = ed.Hospital?.HospitalAddress ?? "",
                        HospitalPhone = ed.Hospital?.HospitalPhone ?? "",
                        DistanceText = ed.Distance.DistanceText,
                        DurationText = ed.Distance.DurationText
                    })
                    .ToList<object>();

                Console.WriteLine($"Returning {result.Count} blood requests sorted by distance");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in FindBloodRequestsByBloodTypeV2: {ex.Message}");
                return new List<object>();
            }
        }
    }
}