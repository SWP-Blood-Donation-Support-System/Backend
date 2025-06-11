namespace BloodDonationAPI.DTO
{
    public class AppointmentHistoryDto
    {
        public int AppointmentHistoryId { get; set; }
        public int? AppointmentId { get; set; }
        public DateTime? AppointmentDate { get; set; }
        public string? AppointmentStatus { get; set; }

        // Thông tin lịch hẹn
        public DateOnly? AppointmentDateOfAppointment { get; set; }
        public TimeOnly? AppointmentTime { get; set; }
        public string? AppointmentTitle { get; set; }
        public string? AppointmentContent { get; set; }
    }


}
