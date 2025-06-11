namespace BloodDonationAPI.DTO.BloodInventory;

public class UseBloodRequestDTO
{
    public string BloodTypeName { get; set; }
    public int RequiredUnits { get; set; }
    public string Note { get; set; }
}

// DTO cho response để hiển thị chi tiết sử dụng máu
public class UseBloodResponseDTO
{
    public string BloodTypeName { get; set; }
    public int TotalUsedUnits { get; set; }
    public List<BloodUsageDetailDTO> UsageDetails { get; set; } = new List<BloodUsageDetailDTO>();
    public string Note { get; set; }
}

public class BloodUsageDetailDTO
{
    public int BloodTypeId { get; set; }
    public int UsedUnits { get; set; }
    public DateOnly ExpiryDate { get; set; }
}
