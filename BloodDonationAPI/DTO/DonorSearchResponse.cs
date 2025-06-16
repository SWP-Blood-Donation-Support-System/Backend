using System;
using System.Collections.Generic;

namespace BloodDonationAPI.DTO
{
    public class DonorSearchResponse
    {
        public List<NearbyDonor> Donors { get; set; } = new List<NearbyDonor>();
    }

    public class NearbyDonor
    {
        public string Id { get; set; }
        public double Distance { get; set; } // Khoảng cách đến vị trí tìm kiếm (km)
        public string BloodType { get; set; }
        public string Status { get; set; } // AVAILABLE, UNAVAILABLE
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
