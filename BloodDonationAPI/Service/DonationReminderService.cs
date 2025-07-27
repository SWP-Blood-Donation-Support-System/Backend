using BloodDonationAPI.DTO;
using Microsoft.EntityFrameworkCore;

namespace BloodDonationAPI.Service
{
    public class DonationReminderService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ReminderSettings _reminderSettings;

        public DonationReminderService(IServiceScopeFactory scopeFactory, ReminderSettings settings)
        {
            _scopeFactory = scopeFactory;
            _reminderSettings = settings;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var bloodDonationProcessServiece = scope.ServiceProvider.GetRequiredService<IBloodDonationProcessService>();
                    var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
                    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                    var context = scope.ServiceProvider.GetRequiredService<BloodDonationSystemContext>();

                    // Existing functionality
                    await bloodDonationProcessServiece.UpdateEligibleUsersAsync();
                    await eventService.CancelPastEventsAsync();
                    
                    // 🆕 New functionality: Send event reminder emails
                    await SendEventReminderEmailsAsync(context, emailService);
                }

                ////thay đổi thời gian thông qua API
                await Task.Delay(_reminderSettings.ReminderInterval, stoppingToken);


                // Chạy mỗi 24 giờ
                //await Task.Delay(TimeSpan.FromHours(24), stoppingToken);

                // 👉 Đổi thành TimeSpan.FromSeconds(30) nếu bạn đang test
                //await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }

        private async Task SendEventReminderEmailsAsync(BloodDonationSystemContext context, IEmailService emailService)
        {
            try
            {
                Console.WriteLine("🔄 Bắt đầu kiểm tra và gửi email nhắc nhở event...");
                
                var tomorrow = DateOnly.FromDateTime(DateTime.Now.AddDays(1));
                
                // Tìm các event diễn ra vào ngày mai
                var upcomingEvents = await context.Events
                    .Where(e => e.EventDate == tomorrow && e.EventStatus == "Public")
                    .ToListAsync();
                
                if (!upcomingEvents.Any())
                {
                    Console.WriteLine("ℹ️ Không có event nào diễn ra vào ngày mai.");
                    return;
                }
                
                Console.WriteLine($"📅 Tìm thấy {upcomingEvents.Count} event diễn ra vào ngày mai.");
                
                foreach (var eventItem in upcomingEvents)
                {
                    Console.WriteLine($"🎯 Xử lý event: {eventItem.EventTitle}");
                    
                    // Tìm các user đã đăng ký event này
                    var registeredUsers = await context.AppointmentRecords
                        .Where(ar => ar.EventId == eventItem.EventId && 
                                   ar.Status == "Đã đủ điều kiện") // Chỉ gửi cho những người đủ điều kiện
                        .Include(ar => ar.UsernameNavigation)
                        .Select(ar => ar.UsernameNavigation)
                        .Where(u => u != null && !string.IsNullOrEmpty(u.Email))
                        .ToListAsync();
                    
                    if (!registeredUsers.Any())
                    {
                        Console.WriteLine($"ℹ️ Không có user nào đăng ký event '{eventItem.EventTitle}' hoặc chưa đủ điều kiện.");
                        continue;
                    }
                    
                    Console.WriteLine($"👥 Tìm thấy {registeredUsers.Count} user đã đăng ký event '{eventItem.EventTitle}'.");
                    
                    // Gửi email nhắc nhở cho từng user
                    foreach (var user in registeredUsers)
                    {
                        try
                        {
                            var reminderDto = new EventReminderDto
                            {
                                UserEmail = user.Email!,
                                UserFullName = user.FullName ?? user.Username,
                                EventTitle = eventItem.EventTitle ?? "Sự kiện hiến máu",
                                EventContent = eventItem.EventContent ?? "Tham gia hiến máu để cứu người",
                                EventDate = eventItem.EventDate ?? tomorrow,
                                EventTime = eventItem.EventTime ?? new TimeOnly(9, 0),
                                Location = eventItem.Location ?? "Địa điểm sẽ được thông báo",
                                BloodTypeRequired = eventItem.BloodTypeRequired ?? ""
                            };
                            
                            var emailSent = await emailService.SendEventReminderEmailAsync(reminderDto);
                            
                            if (emailSent)
                            {
                                Console.WriteLine($"✅ Gửi email nhắc nhở thành công cho {user.Username} ({user.Email})");
                            }
                            else
                            {
                                Console.WriteLine($"❌ Gửi email nhắc nhở thất bại cho {user.Username} ({user.Email})");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"❌ Lỗi khi gửi email nhắc nhở cho {user.Username}: {ex.Message}");
                        }
                    }
                }
                
                Console.WriteLine("✅ Hoàn thành gửi email nhắc nhở event.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi khi xử lý email nhắc nhở event: {ex.Message}");
            }
        }
    }
}
