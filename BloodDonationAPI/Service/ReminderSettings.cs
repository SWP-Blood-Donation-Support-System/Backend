namespace BloodDonationAPI.Service
{
    public class ReminderSettings
    {
        public TimeSpan ReminderInterval { get; set; } = TimeSpan.FromHours(24); // Mặc định là 1 ngày
    }
}
