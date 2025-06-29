namespace BloodDonationAPI.DTO
{
    public class AppointmentNoteDto
    {
        public int AppointmentId { get; set; }
       
        public string ReasonCode { get; set; } = null!;

        public string? CustomNote { get; set; } 
    }
}
