using System;
using System.Collections.Generic;

namespace BloodDonationAPI.DTO
{
    public class DonorSearchResponseDTO
    {
        public List<NearbyDonor> Donors { get; set; } = new List<NearbyDonor>();
    }    public class NearbyDonor
    {
        public string Id { get; set; }
        public string Distance { get; set; } // Khoảng cách đến vị trí tìm kiếm (đã định dạng, ví dụ: "5.2 km" hoặc "800 m")
        public string BloodType { get; set; }
        public string Status { get; set; } // AVAILABLE, UNAVAILABLE
        public string ProfileStatus { get; set; } // ProfileStatus từ bảng User
        public DateTime? LastDonationDate { get; set; }
        public ContactInfo ContactInfo { get; set; }
    }

    public class ContactInfo
    {
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
    }
}
