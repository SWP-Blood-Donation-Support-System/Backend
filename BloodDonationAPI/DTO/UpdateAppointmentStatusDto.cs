namespace BloodDonationAPI.DTO
{
    public class UpdateAppointmentStatusDto
    {
        public int AppointmentHistoryId { get; set; }
        public string AppointmentStatus { get; set; } = null!;// e.g., "Completed", "Cancelled", etc.
    }
}
