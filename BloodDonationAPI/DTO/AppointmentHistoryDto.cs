namespace BloodDonationAPI.DTO
{
    public class AppointmentHistoryDto
    {
        public int AppointmentId { get; set; }
        public int? EventId { get; set; }
        public DateTime? AppointmentDate { get; set; }
        public string? AppointmentStatus { get; set; }
        

        // Thông tin lịch hẹn
        public DateOnly? AppointmentDateOfAppointment { get; set; }
        public TimeOnly? AppointmentTime { get; set; }
        public string? AppointmentTitle { get; set; }
        public string? AppointmentContent { get; set; }

        //staff note
        public string? StaffNote { get; set; }

        //thông tin hiến máu 
        public string? BloodStatus { get; set; }
        public string? BloodType { get; set; }
        public int? DonationUnit { get; set; }
        
        public string? BloodLocation { get; set; }
        // deferral information neu co

        public string? DeferralReasonText { get; set; }
        public string? DeferralAdvice { get; set; }
        public string? DeferralUserNote { get; set; }

        // Ngày có thể hiến lại (nếu không bị hoãn vĩnh viễn)
        public DateOnly? CanDonateAgainDate { get; set; }


    }



}
