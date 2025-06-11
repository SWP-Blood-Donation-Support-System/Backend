using System.Text.Json.Serialization;

namespace BloodDonationAPI.DTO.BloodInventory;

public class UpdateBloodInventoryRequestDTO
{
    public string BloodType { get; set; }
    public int Unit { get; set; }
}
