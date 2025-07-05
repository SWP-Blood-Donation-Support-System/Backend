using System.Collections.Generic;

namespace BloodDonationAPI.DTO
{
    public class BloodRequestSearchResponseDTO
    {
        public List<BloodRequestResult> Requests { get; set; } = new List<BloodRequestResult>();
    }    public class BloodRequestResult
    {
        public string Id { get; set; } = string.Empty;
        public string Distance { get; set; } = string.Empty; // Khoảng cách đã định dạng (ví dụ: "5.2 km" hoặc "800 m")
        public string BloodType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // PENDING, FULFILLED, CANCELLED
        public Location Location { get; set; } = new Location();
        public RequesterInfo RequesterInfo { get; set; } = new RequesterInfo();
    }

    public class RequesterInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class Location
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Address { get; set; } = string.Empty;
    }
}
