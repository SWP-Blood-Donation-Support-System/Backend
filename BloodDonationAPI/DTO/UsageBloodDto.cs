namespace BloodDonationAPI.DTO.BloodInventory;
using System.ComponentModel;

public class UseBloodRequestDTO
{
    public string BloodType { get; set; }
    public int RequiredUnits { get; set; }
    public string Note { get; set; }

    [DefaultValue(1)]
    public int? HospitalId { get; set; }
}

// DTO cho response để hiển thị chi tiết sử dụng máu
public class UseBloodResponseDTO
{
    public string BloodType { get; set; }
    public int TotalUsedUnits { get; set; }
    public List<BloodUsageDetailDTO> UsageDetails { get; set; } = new List<BloodUsageDetailDTO>();
    public string Note { get; set; }
}

public class BloodUsageDetailDTO
{
    public int BloodDetailId { get; set; }
    public int UsedUnits { get; set; }
    public DateOnly BloodDetailDate { get; set; }
}
