namespace BloodDonationAPI.DTO.BloodInventory;
using System.ComponentModel.DataAnnotations;

public class ExpireBloodRequestDTO
{
    [Required(ErrorMessage = "ID chi tiết máu là bắt buộc")]
    public int BloodDetailId { get; set; }
}
