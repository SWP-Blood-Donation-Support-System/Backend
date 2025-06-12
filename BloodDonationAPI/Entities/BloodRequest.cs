using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BloodDonationAPI.Entities
{
    public class BloodRequest
    {
        [Key]
        public int Id { get; set; }
        
        public string BloodType { get; set; }
        
        [Required]
        public string Status { get; set; } = "PENDING"; // PENDING, APPROVED, FULFILLED
        
        [Required]
        public double Latitude { get; set; }
        
        [Required]
        public double Longitude { get; set; }
        
        [Required]
        public string Address { get; set; }
        
        [Required]
        public string RequesterName { get; set; }
        
        public string RequesterPhone { get; set; }
        
        public string RequesterEmail { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime? UpdatedAt { get; set; }
        
        // Foreign key to User if needed
        public string Username { get; set; }
        
        [ForeignKey("Username")]
        public User User { get; set; }
    }
}