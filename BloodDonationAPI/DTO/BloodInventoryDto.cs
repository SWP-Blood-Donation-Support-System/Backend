namespace BloodDonationAPI.DTOs.BloodInventory;

public class BloodInventoryResponseDTO
{
    public List<BloodInventoryItemDTO> Inventory { get; set; } = new List<BloodInventoryItemDTO>();
}

public class BloodInventoryItemDTO
{
    public string BloodTypeName { get; set; }
    public int TotalUnits { get; set; }
    public int AvailableUnits { get; set; }
    public int ExpiredUnits { get; set; }
}
