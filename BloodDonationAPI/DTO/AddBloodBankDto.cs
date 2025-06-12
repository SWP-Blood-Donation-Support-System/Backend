using System.ComponentModel.DataAnnotations;

namespace BloodDonationAPI.DTO
{
    public class AddBloodBankDto
    {
        [Required(ErrorMessage = "Tên nhóm máu không được để trống")]
        public string BloodTypeName { get; set; }

        [Required(ErrorMessage = "Số đơn vị máu là bắt buộc")]
        [Range(1, 1000, ErrorMessage = "Số đơn vị máu phải từ 1 đến 1000")]
        public int Unit { get; set; }

        [Required(ErrorMessage = "Lịch sử hiến máu là bắt buộc")]
        public int DonationHistoryId { get; set; }

        [Required(ErrorMessage = "Ngày hết hạn là bắt buộc")]
        public DateOnly ExpiryDate { get; set; }

        public string? Status { get; set; } // Để null cũng được, xử lý sau
    }
}
