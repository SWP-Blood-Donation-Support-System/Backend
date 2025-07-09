using System.ComponentModel.DataAnnotations;

namespace BloodDonationAPI.DTO
{
    public class UpdateProfileDto
    {
        public DateOnly? DateOfBirth { get; set; }
        
        [StringLength(10)]
        public string? Gender { get; set; }
        
        [Phone]
        [StringLength(15)]
        public string? Phone { get; set; }
        
        [StringLength(500)]
        public string? Address { get; set; }
        
        [StringLength(5)]
        public string? BloodType { get; set; }
        
        [StringLength(50)]
        public string? ProfileStatus { get; set; }
        
        [StringLength(100)]
        public string? FullName { get; set; }
    }
}
