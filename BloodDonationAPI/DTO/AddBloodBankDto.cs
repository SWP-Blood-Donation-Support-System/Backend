using System.ComponentModel.DataAnnotations;

namespace BloodDonationAPI.DTO
{
    public class AddBloodBankDto
    {
        [Required(ErrorMessage = "Tên nhóm máu không được để trống")]
        public string BloodType { get; set; }

        [Required(ErrorMessage = "Số đơn vị máu là bắt buộc")]
        [Range(1, 1000, ErrorMessage = "Số đơn vị máu phải từ 1 đến 1000")]
        public int Volume { get; set; }

        [Required(ErrorMessage = "Ngày hết hạn là bắt buộc")]
        public DateOnly BloodDetailDate { get; set; }

        public string? Note { get; set; }
    }
}
