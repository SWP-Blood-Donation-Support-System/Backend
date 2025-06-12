namespace BloodDonationAPI.DTOs.BloodInventory;

public class BloodBankDTO
{
    public int BloodTypeId { get; set; }
    public string BloodTypeName { get; set; }
    public int? Unit { get; set; }
    public int? DonationHistoryId { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public string Status { get; set; }
}
