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

        public async Task<DonorSearchResponse> FindNearbyDonorsAsync(DonorSearchRequest request)
        {            // Lấy tất cả người dùng có role là User (người hiến máu) và có ProfileStatus Active
            var potentialDonors = await _context.Users
                .Where(u => u.Role == "User" && 
                           u.BloodType == request.BloodType &&
                           u.ProfileStatus == "Active")
                .ToListAsync();

            var nearbyDonors = new List<NearbyDonor>();

            foreach (var donor in potentialDonors)
            {                // Sử dụng API để tính khoảng cách hoặc tính toán gần đúng dựa trên địa chỉ
                // Trong thực tế, bạn có thể sử dụng Google Maps Geocoding API để chuyển đổi địa chỉ thành tọa độ
                // Sau đó tính khoảng cách chính xác với công thức Haversine
                
                // Hiện tại, ta sử dụng khoảng cách mô phỏng thông minh hơn
                double distance = GenerateSmartDistance(request.Lat, request.Lng, donor.Address ?? "", request.Radius);

                // Chỉ lấy những donor trong bán kính yêu cầu
                if (distance <= request.Radius)
                {                    // Lấy lần hiến máu gần nhất từ bảng DonationHistory
                    var lastDonation = await _context.DonationHistories
                        .Where(dh => dh.Username == donor.Username && 
                                    dh.DonationStatus == "Hoàn thành")
                        .OrderByDescending(dh => dh.DonationDate)
                        .FirstOrDefaultAsync();// Xác định trạng thái available/unavailable
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
        }        private string DetermineAvailabilityStatus(DateOnly? lastDonationDate)
        {
            if (!lastDonationDate.HasValue)
                return "AVAILABLE";

            var threeMonthsAgo = DateTime.Now.AddMonths(-3);
            var lastDonationDateTime = lastDonationDate.Value.ToDateTime(TimeOnly.MinValue);
            return lastDonationDateTime <= threeMonthsAgo ? "AVAILABLE" : "UNAVAILABLE";
        }

        /// <summary>
        /// Tạo khoảng cách thông minh dựa trên địa chỉ của donor
        /// </summary>
        /// <param name="lat">Vĩ độ người dùng</param>
        /// <param name="lng">Kinh độ người dùng</param>
        /// <param name="address">Địa chỉ người hiến máu</param>
        /// <param name="maxRadius">Bán kính tối đa</param>
        /// <returns>Khoảng cách ước tính (km)</returns>
        private double GenerateSmartDistance(double lat, double lng, string address, double maxRadius)
        {
            var random = new Random();
            
            // Kiểm tra nếu cùng khu vực, tạo khoảng cách gần hơn
            if (!string.IsNullOrEmpty(address))
            {
                // Giả sử vị trí người dùng là TP.HCM dựa trên lat/lng
                
                // Nếu địa chỉ có chứa TP.HCM hoặc Quận 1, 2, 3... thì tạo khoảng cách gần hơn
                if (address.Contains("TP.HCM") || 
                    address.Contains("TP. HCM") || 
                    address.Contains("HCM") || 
                    address.Contains("Hồ Chí Minh"))
                {
                    // Khoảng cách trong nội thành (0-15km)
                    return random.NextDouble() * 15;
                }
                
                // Nếu trong địa chỉ có chứa "Hà Nội" thì tạo khoảng cách xa hơn
                if (address.Contains("Hà Nội") || address.Contains("Ha Noi"))
                {
                    // Khoảng cách liên tỉnh (500-1000km)
                    return 500 + random.NextDouble() * 500;
                }
            }
            
            // Tạo ngẫu nhiên với phân phối không đều - nhiều người gần hơn, ít người xa hơn
            var randomValue = random.NextDouble();
            // Sử dụng hàm bậc 2 để tạo phân phối không đều
            var normalizedDistance = Math.Pow(randomValue, 2);
            
            return normalizedDistance * maxRadius;
        }
    }
}