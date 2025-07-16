namespace BloodDonationAPI.Service
{
    public class DonationReminderService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public DonationReminderService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var bloodDonationProcessServiece = scope.ServiceProvider.GetRequiredService<IBloodDonationProcessService>();
                    var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();

                    await bloodDonationProcessServiece.UpdateEligibleUsersAsync();
                    await eventService.CancelPastEventsAsync();
                }
                // tu dongj chay moi 24h de cap nhat stus cho user
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken); // Check every 24 hours

                ////👉 đổi thành TimeSpan.FromSeconds(30) nếu bạn đang test
                //await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken); // Check every 30 seconds for testing purposes
            }
        }   
    }
}
