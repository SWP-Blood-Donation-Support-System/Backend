namespace BloodDonationAPI.DTO
{
    public class BloodCompareResultDto
    {
        public bool IsEnough { get; set; } // true: đủ, false: không đủ
        public string Status => IsEnough ? "đủ" : "không đủ";
        public int? RequiredUnits { get; set; }
        public int? AvailableUnits { get; set; }
        public List<BloodDetailInfo>? Details { get; set; } // chỉ trả về khi đủ
        public class BloodDetailInfo
        {
            public int BloodDetailId { get; set; }
            public string BloodType { get; set; }
            public int Volume { get; set; }
            public DateOnly? BloodDetailDate { get; set; }
        }
    }
} 