namespace BloodDonationAPI.DTOs.BloodInventory;

public class BloodBankDTO
{
    public string BloodType { get; set; }
    public int? BloodVolumeTotal { get; set; }  // Tổng số lượng từ BloodBank
    public string? BloodBankStatus { get; set; }
}
