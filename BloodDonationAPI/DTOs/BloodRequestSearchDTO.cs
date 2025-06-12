using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BloodDonationAPI.DTOs
{
    public class BloodRequestSearchRequest
    {
        public string BloodType { get; set; }
        
        [Required]
        public double Lat { get; set; }
        
        [Required]
        public double Lng { get; set; }
        
        [Required]
        public double Radius { get; set; }
    }

    public class Location
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Address { get; set; }
    }

    public class RequesterInfo
    {
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
    }
    
    public class BloodRequestResult
    {
        public string Id { get; set; }
        public double Distance { get; set; }
        public string BloodType { get; set; }
        public string Status { get; set; }
        public Location Location { get; set; }
        public RequesterInfo RequesterInfo { get; set; }
    }

    public class BloodRequestSearchResponse
    {
        public List<BloodRequestResult> Requests { get; set; } = new List<BloodRequestResult>();
    }
}