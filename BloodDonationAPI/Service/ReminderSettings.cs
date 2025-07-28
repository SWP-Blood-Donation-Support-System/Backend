namespace BloodDonationAPI.Service
{
    public class ReminderSettings
    {
        public TimeSpan _reminderInterval { get; set; } = TimeSpan.FromHours(24); // Mặc định là 1 ngày
        private CancellationTokenSource _cts = new CancellationTokenSource();

        public TimeSpan ReminderInterval
        {
            get => _reminderInterval;
            set
            {
                _reminderInterval = value;
                // Khi thay đổi thì hủy vòng chờ hiện tại
                _cts.Cancel();
                _cts = new CancellationTokenSource();
            }
        }

        public CancellationToken Token => _cts.Token;
    }
}
