using System;
using System.Collections.Generic;

namespace BloodDonationAPI.DTOs
{
    // DTOs cho chức năng tìm người hiến máu
    public class DonorSearchRequest
{
    public string BloodType { get; set; } = string.Empty;
    public double Lat { get; set; }
    public double Lng { get; set; }
    public double Radius { get; set; }
}

public class DonorSearchResponse
{
    public List<NearbyDonor> Donors { get; set; } = new List<NearbyDonor>();
}

public class NearbyDonor
{
    public string Id { get; set; } = string.Empty;
    public double Distance { get; set; }
    public string BloodType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? LastDonationDate { get; set; }
    public ContactInfo ContactInfo { get; set; } = new ContactInfo();
}

public class ContactInfo
{
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}
}