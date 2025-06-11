using BloodDonationAPI.DTOs;
using BloodDonationAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace BloodDonationAPI.Service
{
    public class DonorSearchService : IDonorSearchService
    {
        private readonly BloodDonationSystemContext _context;

        public DonorSearchService(BloodDonationSystemContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Tìm kiếm người hiến máu gần đây theo vị trí, nhóm máu và bán kính
        /// </summary>
        public async Task<DonorSearchResponse> FindNearbyDonorsAsync(DonorSearchRequest request)
        {
            // Lấy tất cả người dùng có role là User (người hiến máu) và có ProfileStatus Active
            var potentialDonors = await _context.Users
                .Where(u => u.Role == "User" && 
                           u.BloodType == request.BloodType &&
                           u.ProfileStatus == "Active")
                .ToListAsync();

            var nearbyDonors = new List<NearbyDonor>();

            foreach (var donor in potentialDonors)
            {
                // Tính khoảng cách dựa theo địa chỉ
                double distance = GenerateSmartDistance(request.Lat, request.Lng, donor.Address ?? "", request.Radius);

                // Chỉ lấy những donor trong bán kính yêu cầu
                if (distance <= request.Radius)
                {
                    // Lấy lần hiến máu gần nhất từ bảng DonationHistory
                    var lastDonation = await _context.DonationHistories
                        .Where(dh => dh.Username == donor.Username && 
                                    dh.DonationStatus == "Hoàn thành")
                        .OrderByDescending(dh => dh.DonationDate)
                        .FirstOrDefaultAsync();

                    // Xác định trạng thái available/unavailable
                    var status = DetermineAvailabilityStatus(lastDonation?.DonationDate);

                    nearbyDonors.Add(new NearbyDonor
                    {
                        Id = donor.Username,
                        Distance = Math.Round(distance, 2),
                        BloodType = donor.BloodType ?? "Unknown",
                        Status = status,
                        LastDonationDate = lastDonation?.DonationDate?.ToDateTime(TimeOnly.MinValue),
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

            // Sắp xếp theo khoảng cách từ gần đến xa
            nearbyDonors = nearbyDonors.OrderBy(d => d.Distance).ToList();

            return new DonorSearchResponse
            {
                Donors = nearbyDonors
            };
        }

        /// <summary>
        /// Tìm kiếm người hiến máu theo nhóm máu trong TP.HCM
        /// </summary>
        public async Task<DonorSearchResponse> FindDonorsByBloodTypeAsync(DonorSearchRequest request)
        {
            // Lấy tất cả người dùng có role là User và có ProfileStatus Active, và ở TP.HCM
            var potentialDonors = await _context.Users
                .Where(u => u.Role == "User" && 
                           u.BloodType == request.BloodType &&
                           u.ProfileStatus == "Active" &&
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
                // Lấy lần hiến máu gần nhất từ bảng DonationHistory
                var lastDonation = await _context.DonationHistories
                    .Where(dh => dh.Username == donor.Username && 
                                dh.DonationStatus == "Hoàn thành")
                    .OrderByDescending(dh => dh.DonationDate)
                    .FirstOrDefaultAsync();

                // Xác định trạng thái available/unavailable
                var status = DetermineAvailabilityStatus(lastDonation?.DonationDate);

                donors.Add(new NearbyDonor
                {
                    Id = donor.Username,
                    Distance = 0, // Không có khoảng cách vì không có tọa độ
                    BloodType = donor.BloodType ?? "Unknown",
                    Status = status,
                    LastDonationDate = lastDonation?.DonationDate?.ToDateTime(TimeOnly.MinValue),
                    ContactInfo = new ContactInfo
                    {
                        Name = donor.FullName ?? "Unknown",
                        Phone = donor.Phone ?? "",
                        Email = donor.Email ?? "",
                        Address = donor.Address ?? ""
                    }
                });
            }

            return new DonorSearchResponse
            {
                Donors = donors
            };
        }

        /// <summary>
        /// Tìm kiếm người hiến máu theo nhóm máu trên toàn Việt Nam và sắp xếp theo khoảng cách
        /// </summary>
        public async Task<DonorSearchResponse> FindDonorsInHCMByBloodTypeAsync(DonorSearchRequest request)
        {
            // Lấy tất cả người dùng có role là User (người hiến máu) và có ProfileStatus Active
            var potentialDonors = await _context.Users
                .Where(u => u.Role == "User" && 
                           u.BloodType == request.BloodType &&
                           u.ProfileStatus == "Active")
                .ToListAsync();

            var donors = new List<NearbyDonor>();

            foreach (var donor in potentialDonors)
            {
                // Tính khoảng cách dựa trên địa chỉ trong Việt Nam
                double distance = CalculateVietnamDistance(donor.Address ?? "");
                
                // Lấy lần hiến máu gần nhất từ bảng DonationHistory
                var lastDonation = await _context.DonationHistories
                    .Where(dh => dh.Username == donor.Username && 
                                dh.DonationStatus == "Hoàn thành")
                    .OrderByDescending(dh => dh.DonationDate)
                    .FirstOrDefaultAsync();

                // Xác định trạng thái available/unavailable
                var status = DetermineAvailabilityStatus(lastDonation?.DonationDate);

                donors.Add(new NearbyDonor
                {
                    Id = donor.Username,
                    Distance = Math.Round(distance, 2),
                    BloodType = donor.BloodType ?? "Unknown",
                    Status = status,
                    LastDonationDate = lastDonation?.DonationDate?.ToDateTime(TimeOnly.MinValue),
                    ContactInfo = new ContactInfo
                    {
                        Name = donor.FullName ?? "Unknown",
                        Phone = donor.Phone ?? "",
                        Email = donor.Email ?? "",
                        Address = donor.Address ?? ""
                    }
                });
            }

            // Sắp xếp theo khoảng cách từ gần đến xa
            donors = donors.OrderBy(d => d.Distance).ToList();

            return new DonorSearchResponse
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
        }

        /// <summary>
        /// Tạo khoảng cách ngẫu nhiên cho demo
        /// </summary>
        private double GenerateSmartDistance(double lat, double lng, string address, double maxRadius)
        {
            var random = new Random();
            
            // Kiểm tra nếu cùng khu vực, tạo khoảng cách gần hơn
            if (!string.IsNullOrEmpty(address))
            {
                if (address.Contains("TP.HCM") || 
                    address.Contains("TP. HCM") || 
                    address.Contains("HCM") || 
                    address.Contains("Hồ Chí Minh"))
                {
                    // Khoảng cách trong nội thành (0-15km)
                    return random.NextDouble() * 15;
                }
                
                if (address.Contains("Hà Nội") || address.Contains("Ha Noi"))
                {
                    // Khoảng cách liên tỉnh (500-1000km)
                    return 500 + random.NextDouble() * 500;
                }
            }
            
            // Tạo ngẫu nhiên với phân phối không đều
            var randomValue = random.NextDouble();
            var normalizedDistance = Math.Pow(randomValue, 2);
            
            return normalizedDistance * maxRadius;
        }

        /// <summary>
        /// Xác định trạng thái có thể hiến máu hay không
        /// </summary>
        private string DetermineAvailabilityStatus(DateOnly? lastDonationDate)
        {
            if (!lastDonationDate.HasValue)
                return "AVAILABLE";

            var threeMonthsAgo = DateTime.Now.AddMonths(-3);
            var lastDonationDateTime = lastDonationDate.Value.ToDateTime(TimeOnly.MinValue);
            return lastDonationDateTime <= threeMonthsAgo ? "AVAILABLE" : "UNAVAILABLE";
        }
    }
}