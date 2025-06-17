namespace BloodDonationAPI.DTOs.BloodInventory;

public class BloodInventoryResponseDTO
{
    public List<BloodInventoryItemDTO> Inventory { get; set; } = new List<BloodInventoryItemDTO>();
}

public class BloodInventoryItemDTO
{
    public int BloodDetailId { get; set; }  // ID của bản ghi chi tiết
    public string BloodType { get; set; }
    public int Volume { get; set; }  // Số lượng của bản ghi này
    public DateOnly? BloodDetailDate { get; set; }  // Ngày hết hạn
    public string? BloodDetailStatus { get; set; }  // Trạng thái
    public string? Note { get; set; }  // Ghi chú
    public int? HospitalId { get; set; }
}

public class BloodDetailDto
{
    public int BloodDetailId { get; set; }
    public string BloodType { get; set; }
    public int Volume { get; set; }
    public DateOnly BloodDetailDate { get; set; }
    public string BloodDetailStatus { get; set; }
    public string? Note { get; set; }
    public int AppointmentId { get; set; }
    public int HospitalId { get; set; }
}
