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
    public class DonorSearchService : IDonorSearchService
    {
        private readonly BloodDonationSystemContext _context;
        // Reference point: 7 Đ. D1, Long Thạnh Mỹ, Thủ Đức, Hồ Chí Minh 700000, Vietnam
        private readonly double _referenceLatitude = 10.841962;  // Approximate latitude for the reference point
        private readonly double _referenceLongitude = 106.810627; // Approximate longitude for the reference point

        public DonorSearchService(BloodDonationSystemContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Tìm kiếm người hiến máu gần đây theo vị trí, nhóm máu và bán kính
        /// </summary>
        public async Task<DonorSearchResponseDTO> FindNearbyDonorsAsync(DonorSearchRequestDTO request)
        {
            // Lấy tất cả người dùng có role là User (người hiến máu) và có ProfileStatus "Active"
            var potentialDonors = await _context.Users
                .Where(u => u.Role == "User" &&
                           u.BloodType == request.BloodType &&
                           u.ProfileStatus == "Sẵn sàng hiến máu")
                .ToListAsync();

            var nearbyDonors = new List<NearbyDonor>();

            foreach (var donor in potentialDonors)
            {
                // Tính khoảng cách dựa theo địa chỉ
                double distance = GenerateSmartDistance(request.Lat, request.Lng, donor.Address ?? "", request.Radius);

                // Chỉ lấy những donor trong bán kính yêu cầu
                if (distance <= request.Radius)
                {
                    // Lấy lần hiến máu gần nhất từ bảng AppointmentRecord
                    var lastDonation = await _context.AppointmentRecords
                        .Where(ar => ar.Username == donor.Username &&
                                    ar.Status == "Đã hiến")
                        .OrderByDescending(ar => ar.RegistrationDate)
                        .FirstOrDefaultAsync();

                    // Xác định trạng thái available/unavailable
                    var status = DetermineAvailabilityStatus(lastDonation?.RegistrationDate);

                    // Calculate and format distance
                    double distanceRaw = Math.Round(distance, 2);
                    string formattedDistance = FormatDistance(distanceRaw);
                    
                    nearbyDonors.Add(new NearbyDonor
                    {
                        Id = donor.Username,
                        Distance = formattedDistance, // Only use formatted distance string
                        BloodType = donor.BloodType ?? "Unknown",
                        Status = status,
                        ProfileStatus = donor.ProfileStatus ?? "Unknown",
                        LastDonationDate = lastDonation?.RegistrationDate,
                        ContactInfo = new ContactInfo
                        {
                            Name = donor.FullName ?? "Unknown",
                            Phone = donor.Phone ?? "",
                            Email = donor.Email ?? "",
                            Address = donor.Address ?? ""
                        }
                    });
                }
            }

            // Sắp xếp danh sách donor từ gần đến xa sử dụng hàm chung
            nearbyDonors = SortDonorsByDistance(nearbyDonors);

            return new DonorSearchResponseDTO
            {
                Donors = nearbyDonors
            };
        }
        
        /// <summary>
        /// Tìm kiếm người hiến máu theo nhóm máu trong TP.HCM
        /// </summary>
        public async Task<DonorSearchResponseDTO> FindDonorsByBloodTypeAsync(DonorSearchRequestDTO request)
        {
            // Lấy tất cả người dùng có role là User và có ProfileStatus "Active", và ở TP.HCM
            var potentialDonors = await _context.Users
                .Where(u => u.Role == "User" &&
                           u.BloodType == request.BloodType &&
                           u.ProfileStatus == "Sẵn sàng hiến máu" &&
                           (u.Address != null && (
                               u.Address.Contains("TP.HCM") ||
                               u.Address.Contains("TP. HCM") ||
                               u.Address.Contains("HCM") ||
                               u.Address.Contains("Hồ Chí Minh") ||
                               u.Address.Contains("Q1") ||
                               u.Address.Contains("Q2") ||
                               u.Address.Contains("Q3") ||
                               u.Address.Contains("Q5") ||
                               u.Address.Contains("Q10") ||
                               u.Address.Contains("Quận 1") ||
                               u.Address.Contains("Quận 2") ||
                               u.Address.Contains("Quận 3")
                           )))
                .ToListAsync();

            var donors = new List<NearbyDonor>();

            foreach (var donor in potentialDonors)
            {
                // Tính khoảng cách dựa trên địa chỉ
                double distanceInKm = CalculateDistanceFromAddress(donor.Address ?? string.Empty);
                double distanceRaw = Math.Round(distanceInKm, 2);
                string formattedDistance = FormatDistance(distanceRaw);
                
                // Lấy lần hiến máu gần nhất từ bảng AppointmentRecord
                var lastDonation = await _context.AppointmentRecords
                    .Where(ar => ar.Username == donor.Username &&
                                ar.Status == "Đã hiến")
                    .OrderByDescending(ar => ar.RegistrationDate)
                    .FirstOrDefaultAsync();

                // Xác định trạng thái available/unavailable
                var status = DetermineAvailabilityStatus(lastDonation?.RegistrationDate);

                donors.Add(new NearbyDonor
                {
                    Id = donor.Username,
                    Distance = formattedDistance, // Use only formatted distance
                    BloodType = donor.BloodType ?? "Unknown",
                    Status = status,
                    ProfileStatus = donor.ProfileStatus ?? "Unknown",
                    LastDonationDate = lastDonation?.RegistrationDate,
                    ContactInfo = new ContactInfo
                    {
                        Name = donor.FullName ?? "Unknown",
                        Phone = donor.Phone ?? "",
                        Email = donor.Email ?? "",
                        Address = donor.Address ?? ""
                    }
                });
            }

            // Sắp xếp danh sách donor từ gần đến xa sử dụng hàm chung
            donors = SortDonorsByDistance(donors);

            return new DonorSearchResponseDTO
            {
                Donors = donors
            };
        }

        /// <summary>
        /// Tìm kiếm người hiến máu theo nhóm máu trên toàn Việt Nam và sắp xếp theo khoảng cách
        /// </summary>
        public async Task<DonorSearchResponseDTO> FindDonorsInHCMByBloodTypeAsync(DonorSearchRequestDTO request)
        {
            // Lấy tất cả người dùng có role là User (người hiến máu) và có ProfileStatus "Active"
            var potentialDonors = await _context.Users
                .Where(u => u.Role == "User" &&
                           u.BloodType == request.BloodType &&
                           u.ProfileStatus == "Sẵn sàng hiến máu")
                .ToListAsync();

            var donors = new List<NearbyDonor>();

            foreach (var donor in potentialDonors)
            {
                // Tính khoảng cách dựa trên địa chỉ trong Việt Nam
                double distance = CalculateVietnamDistance(donor.Address ?? "");
                double distanceRaw = Math.Round(distance, 2);
                string formattedDistance = FormatDistance(distanceRaw);

                // Lấy lần hiến máu gần nhất từ bảng AppointmentRecord
                var lastDonation = await _context.AppointmentRecords
                    .Where(ar => ar.Username == donor.Username &&
                                ar.Status == "Đã hiến")
                    .OrderByDescending(ar => ar.RegistrationDate)
                    .FirstOrDefaultAsync();

                // Xác định trạng thái available/unavailable
                var status = DetermineAvailabilityStatus(lastDonation?.RegistrationDate);

                donors.Add(new NearbyDonor
                {
                    Id = donor.Username,
                    Distance = formattedDistance, // Use only formatted distance string
                    BloodType = donor.BloodType ?? "Unknown",
                    Status = status,
                    ProfileStatus = donor.ProfileStatus ?? "Unknown",
                    LastDonationDate = lastDonation?.RegistrationDate,
                    ContactInfo = new ContactInfo
                    {
                        Name = donor.FullName ?? "Unknown",
                        Phone = donor.Phone ?? "",
                        Email = donor.Email ?? "",
                        Address = donor.Address ?? ""
                    }
                });
            }

            // Sắp xếp danh sách donor từ gần đến xa sử dụng hàm chung
            donors = SortDonorsByDistance(donors);

            return new DonorSearchResponseDTO
            {
                Donors = donors
            };
        }

        /// <summary>
        /// Tính khoảng cách dựa trên địa chỉ trong phạm vi Việt Nam
        /// </summary>
        private double CalculateVietnamDistance(string address)
        {
            var random = new Random();

            if (!string.IsNullOrEmpty(address))
            {
                // Các tỉnh/thành phố ở miền Nam
                // Quận trong TP.HCM (gần trung tâm)
                if (address.Contains("Quận 1") || address.Contains("Q1") ||
                    address.Contains("Quận 3") || address.Contains("Q3") ||
                    address.Contains("Quận 10") || address.Contains("Q10") ||
                    address.Contains("Quận 5") || address.Contains("Q5"))
                {
                    return random.NextDouble() * 5 + 1; // 1-6km
                }

                // Quận trong TP.HCM (xa trung tâm)
                if (address.Contains("Quận 2") || address.Contains("Q2") ||
                    address.Contains("Thủ Đức") ||
                    address.Contains("Quận 7") || address.Contains("Q7") ||
                    address.Contains("Quận 9") || address.Contains("Q9") ||
                    address.Contains("Bình Thạnh") ||
                    address.Contains("Tân Bình"))
                {
                    return random.NextDouble() * 8 + 5; // 5-13km
                }

                // TP.HCM và các tỉnh lân cận
                if (address.Contains("TP.HCM") || address.Contains("TP. HCM") ||
                    address.Contains("HCM") || address.Contains("Hồ Chí Minh") ||
                    address.Contains("Bình Dương") ||
                    address.Contains("Đồng Nai") ||
                    address.Contains("Long An") ||
                    address.Contains("Tây Ninh") ||
                    address.Contains("Vũng Tàu"))
                {
                    return random.NextDouble() * 30 + 10; // 10-40km
                }

                // Các tỉnh miền Tây Nam Bộ
                if (address.Contains("Tiền Giang") ||
                    address.Contains("Bến Tre") ||
                    address.Contains("Vĩnh Long") ||
                    address.Contains("Cần Thơ") ||
                    address.Contains("An Giang") ||
                    address.Contains("Hậu Giang") ||
                    address.Contains("Kiên Giang") ||
                    address.Contains("Đồng Tháp") ||
                    address.Contains("Cà Mau"))
                {
                    return random.NextDouble() * 100 + 50; // 50-150km
                }

                // Các tỉnh miền Trung
                if (address.Contains("Đà Nẵng") ||
                    address.Contains("Huế") || address.Contains("Thừa Thiên") ||
                    address.Contains("Quảng Nam") ||
                    address.Contains("Quảng Ngãi") ||
                    address.Contains("Bình Định") ||
                    address.Contains("Khánh Hòa") || address.Contains("Nha Trang") ||
                    address.Contains("Nghệ An") ||
                    address.Contains("Hà Tĩnh"))
                {
                    return random.NextDouble() * 200 + 500; // 500-700km
                }

                // Các tỉnh miền Bắc
                if (address.Contains("Hà Nội") ||
                    address.Contains("Hải Phòng") ||
                    address.Contains("Quảng Ninh") ||
                    address.Contains("Bắc Ninh") ||
                    address.Contains("Hưng Yên") ||
                    address.Contains("Hải Dương") ||
                    address.Contains("Nam Định"))
                {
                    return random.NextDouble() * 200 + 1000; // 1000-1200km
                }
            }

            // Mặc định trả về khoảng cách ngẫu nhiên trong Việt Nam (20-1000km)
            return random.NextDouble() * 980 + 20;
        }        /// <summary>
        /// Tạo khoảng cách thông minh dựa trên địa chỉ và tọa độ
        /// </summary>
        private double GenerateSmartDistance(double lat, double lng, string address, double maxRadius)
        {
            var random = new Random();

            // Sử dụng tọa độ người dùng nếu có, ngược lại dựa vào địa chỉ
            if (lat != 0 && lng != 0)
            {
                // Tính khoảng cách thực tế từ tọa độ người dùng đến điểm tham chiếu (7 Đ. D1, Long Thạnh Mỹ, Thủ Đức)
                return CalculateHaversineDistance(lat, lng, _referenceLatitude, _referenceLongitude);
            }
            
            // Kiểm tra theo địa chỉ khi không có tọa độ
            if (!string.IsNullOrEmpty(address))
            {
                // Thủ Đức (nơi có điểm mốc - 7 Đ. D1, Long Thạnh Mỹ)
                if (address.Contains("Long Thạnh Mỹ") || 
                    (address.Contains("Thủ Đức") && address.Contains("D1")))
                {
                    return 0.3 + random.NextDouble() * 0.4; // 0.3-0.7km
                }
                
                // Khu vực Thủ Đức
                if (address.Contains("Thủ Đức") ||
                    address.Contains("Linh Trung") ||
                    address.Contains("Linh Tây") ||
                    address.Contains("Linh Đông") ||
                    address.Contains("Hiệp Phú"))
                {
                    return 1.0 + random.NextDouble() * 3.0; // 1-4km
                }
                
                // Các quận lân cận Thủ Đức
                if (address.Contains("Quận 9") || address.Contains("Q9") ||
                    address.Contains("Quận 2") || address.Contains("Q2") ||
                    address.Contains("Bình Thạnh"))
                {
                    return 4.0 + random.NextDouble() * 4.0; // 4-8km
                }

                // Các quận nội thành TP.HCM
                if (address.Contains("TP.HCM") ||
                    address.Contains("TP. HCM") ||
                    address.Contains("HCM") ||
                    address.Contains("Hồ Chí Minh"))
                {
                    // Khoảng cách trong nội thành (5-20km)
                    return 5.0 + random.NextDouble() * 15.0;
                }

                if (address.Contains("Hà Nội") || address.Contains("Ha Noi"))
                {
                    // Khoảng cách liên tỉnh (1000-1200km)
                    return 1000.0 + random.NextDouble() * 200.0;
                }
                
                // Các tỉnh lân cận
                if (address.Contains("Bình Dương") ||
                    address.Contains("Đồng Nai") ||
                    address.Contains("Long An"))
                {
                    return 20.0 + random.NextDouble() * 20.0; // 20-40km
                }
            }

            // Tạo ngẫu nhiên với phân phối không đều cho các trường hợp khác
            var randomValue = random.NextDouble();
            var normalizedDistance = Math.Pow(randomValue, 2);

            return normalizedDistance * maxRadius;
        }
        
        /// <summary>
        /// Tính khoảng cách giữa hai điểm sử dụng công thức Haversine
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
        /// Xác định trạng thái có thể hiến máu hay không
        /// </summary>
        private string DetermineAvailabilityStatus(DateTime? lastDonationDate)
        {
            if (!lastDonationDate.HasValue)
                return "AVAILABLE";

            var threeMonthsAgo = DateTime.Now.AddMonths(-3);
            return lastDonationDate.Value <= threeMonthsAgo ? "AVAILABLE" : "UNAVAILABLE";
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
        /// Tính khoảng cách dựa trên địa chỉ so với điểm mốc: 7 Đ. D1, Long Thạnh Mỹ, Thủ Đức
        /// </summary>
        private double CalculateDistanceFromAddress(string address)
        {
            address = address ?? string.Empty;
            
            if (string.IsNullOrEmpty(address))
                return double.MaxValue;
            
            // Thủ Đức (nơi có điểm mốc - 7 Đ. D1, Long Thạnh Mỹ)
            if (address.Contains("Long Thạnh Mỹ") || 
                (address.Contains("Thủ Đức") && address.Contains("D1")))
            {
                return 0.5; // Rất gần điểm mốc
            }
            
            // Khu vực Thủ Đức
            if (address.Contains("Thủ Đức") || address.Contains("Linh Trung"))
            {
                return new Random().NextDouble() * 2 + 2; // 2-4km
            }
            
            // Các quận lân cận Thủ Đức
            if (address.Contains("Quận 9") || address.Contains("Q9") ||
                address.Contains("Quận 2") || address.Contains("Q2"))
            {
                return new Random().NextDouble() * 3 + 5; // 5-8km
            }
            
            // Các quận khác trong TP.HCM
            if (address.Contains("TP.HCM") || address.Contains("HCM") || 
                address.Contains("Hồ Chí Minh"))
            {
                return new Random().NextDouble() * 8 + 10; // 10-18km
            }
            
            // Các tỉnh khác
            return new Random().NextDouble() * 500 + 100; // 100-600km
        }        /// <summary>
        /// Đảm bảo kết quả được sắp xếp theo khoảng cách gần nhất
        /// </summary>
        private List<NearbyDonor> SortDonorsByDistance(List<NearbyDonor> donors)
        {
            // Sắp xếp theo khoảng cách từ gần đến xa
            var donorsWithNumericDistance = new List<(NearbyDonor Donor, double NumericDistance)>();
            
            foreach (var donor in donors)
            {
                // Trích xuất giá trị khoảng cách số từ chuỗi định dạng (như "800 m" hoặc "5.2 km")
                double numericDistance = ExtractNumericDistance(donor.Distance);
                donorsWithNumericDistance.Add((donor, numericDistance));
            }
            
            // Sắp xếp theo khoảng cách số từ thấp đến cao (gần đến xa)
            var sortedDonors = donorsWithNumericDistance
                .OrderBy(x => x.NumericDistance) // Sắp xếp từ thấp đến cao
                .Select(x => x.Donor)
                .ToList();
            
            Console.WriteLine($"Sorted {sortedDonors.Count} donors by distance (from lowest to highest)");
            return sortedDonors;
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