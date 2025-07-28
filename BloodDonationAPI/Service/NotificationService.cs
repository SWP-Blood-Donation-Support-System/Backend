using BloodDonationAPI.DTO;
using BloodDonationAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace BloodDonationAPI.Service
{
    public class NotificationService : INotificationService
    {
        private readonly BloodDonationSystemContext _context;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(BloodDonationSystemContext context, ILogger<NotificationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<NotificationDto>> GetNotifications()
        {
            try
            {
                var notifications = await _context.Notifications
                    .Include(n => n.Emergency)
                        .ThenInclude(e => e.Hospital)
                    .Include(n => n.NotificationRecipients)
                        .ThenInclude(nr => nr.UsernameNavigation)
                    .Select(n => new NotificationDto
                    {
                        NotificationId = n.NotificationId,
                        EmergencyId = n.EmergencyId.Value,
                        NotificationStatus = n.NotificationStatus,
                        NotificationTitle = n.NotificationTitle,
                        NotificationContent = n.NotificationContent,
                        NotificationDate = n.NotificationDate.Value,
                        BloodType = n.Emergency.BloodType,
                        RequiredUnits = n.Emergency.RequiredUnits.Value,
                        HospitalName = n.Emergency.Hospital.HospitalName,
                        Recipients = n.NotificationRecipients.Select(nr => new NotificationRecipientDto
                        {
                            Username = nr.Username,
                            FullName = nr.UsernameNavigation.FullName,
                            ResponseStatus = nr.ResponseStatus,
                            ResponseDate = nr.ResponseDate,
                            ResponseGo = nr.ResponseGo,
                            ResponseTime = nr.ResponseTime
                        }).ToList()
                    })
                    .OrderByDescending(n => n.NotificationDate)
                    .ToListAsync();

                return notifications;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notifications");
                throw;
            }
        }

        private string NormalizeProvinceName(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            var s = input.Trim().ToLower();
            if (s.Contains("hồ chí minh") || s.Contains("sài gòn") || s.Contains("tp.hcm") || s.Contains("tp. hcm") || s.Contains("tp. hồ chí minh") || s.Contains("thành phố hồ chí minh"))
                return "TP. HCM";
            return s;
        }

        public async Task<string> CreateNotificationForEmergency(int emergencyId)
        {
            try
            {
                var emergency = await _context.Emergencies
                    .Include(e => e.Hospital)
                    .FirstOrDefaultAsync(e => e.EmergencyId == emergencyId);

                if (emergency == null)
                    return "Emergency not found.";

                if (emergency.EmergencyStatus != "Đã xét duyệt")
                    return "Emergency must be approved first.";

                // Kiểm tra kho máu có đủ không
                var availableBlood = await _context.BloodDetails
                    .Where(b => b.BloodType == emergency.BloodType && 
                               b.BloodDetailStatus == "Còn hạn" && 
                               b.Volume > 0)
                    .SumAsync(b => b.Volume);

                // Nếu đủ máu trong kho thì không gửi thông báo
                if (availableBlood >= emergency.RequiredUnits)
                {
                    return "Sufficient blood available in inventory. No notification needed.";
                }

                // Lấy tỉnh thành từ địa chỉ bệnh viện và chuẩn hóa
                var hospitalProvince = NormalizeProvinceName(emergency.Hospital.HospitalAddress?.Split(',').LastOrDefault());
                // Lấy user phù hợp nhóm máu, có địa chỉ và ProfileStatus là "Sẵn sàng hiến máu"
                var users = await _context.Users
                    .Where(u => u.BloodType == emergency.BloodType && u.Address != null && u.ProfileStatus == "Sẵn sàng hiến máu")
                    .ToListAsync();
                // Lọc lại bằng LINQ to Objects với chuẩn hóa địa chỉ
                var matchingUsers = users
                    .Where(u => NormalizeProvinceName(u.Address.Split(',').LastOrDefault()) == hospitalProvince)
                    .ToList();

                var notification = new Notification
                {
                    EmergencyId = emergencyId,
                    NotificationStatus = "Đã gửi",
                    NotificationTitle = $"Yêu cầu hiến máu khẩn cấp - {emergency.Hospital.HospitalName}",
                    NotificationContent = $"Cần {emergency.RequiredUnits} đơn vị nhóm máu {emergency.BloodType} tại {emergency.Hospital.HospitalName}",
                    NotificationDate = DateOnly.FromDateTime(DateTime.Now)
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                foreach (var user in matchingUsers)
                {
                    var recipient = new NotificationRecipient
                    {
                        NotificationId = notification.NotificationId,
                        Username = user.Username,
                        ResponseStatus = "Chưa phản hồi",
                        ResponseDate = null,
                        ResponseGo = null,
                        ResponseTime = null
                    };
                    _context.NotificationRecipients.Add(recipient);
                }

                await _context.SaveChangesAsync();
                return "Notification created successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification");
                throw;
            }
        }

        public async Task<List<NotificationDto>> GetNotificationsByBloodType(string bloodType)
        {
            try
            {
                return await _context.Notifications
                    .Include(n => n.Emergency)
                        .ThenInclude(e => e.Hospital)
                    .Where(n => n.Emergency.BloodType == bloodType)
                    .Select(n => new NotificationDto
                    {
                        NotificationId = n.NotificationId,
                        EmergencyId = n.EmergencyId.Value,
                        NotificationStatus = n.NotificationStatus,
                        NotificationTitle = n.NotificationTitle,
                        NotificationContent = n.NotificationContent,
                        NotificationDate = n.NotificationDate.Value,
                        BloodType = n.Emergency.BloodType,
                        RequiredUnits = n.Emergency.RequiredUnits.Value,
                        HospitalName = n.Emergency.Hospital.HospitalName
                    })
                    .OrderByDescending(n => n.NotificationDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notifications by blood type");
                throw;
            }
        }

        public async Task<List<NotificationDto>> GetUserNotifications(string username)
        {
            try
            {
                return await _context.NotificationRecipients
                    .Include(nr => nr.Notification)
                        .ThenInclude(n => n.Emergency)
                            .ThenInclude(e => e.Hospital)
                    .Where(nr => nr.Username == username)
                    .Select(nr => new NotificationDto
                    {
                        NotificationId = nr.Notification.NotificationId,
                        EmergencyId = nr.Notification.EmergencyId.Value,
                        NotificationStatus = nr.Notification.NotificationStatus,
                        NotificationTitle = nr.Notification.NotificationTitle,
                        NotificationContent = nr.Notification.NotificationContent,
                        NotificationDate = nr.Notification.NotificationDate.Value,
                        BloodType = nr.Notification.Emergency.BloodType,
                        RequiredUnits = nr.Notification.Emergency.RequiredUnits.Value,
                        HospitalName = nr.Notification.Emergency.Hospital.HospitalName,
                        ResponseStatus = nr.ResponseStatus,
                        ResponseDate = nr.ResponseDate
                    })
                    .OrderByDescending(n => n.NotificationDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user notifications");
                throw;
            }
        }

        public async Task<string> UpdateNotificationResponse(int notificationId, string username, string responseStatus, DateOnly? responseGo, TimeOnly? responseTime)
        {
            try
            {
                if (string.IsNullOrEmpty(responseStatus))
                    return "Response status is required.";

                if (responseStatus != "Chấp nhận" && responseStatus != "Từ chối")
                    return "Invalid response status. Must be either 'Chấp nhận' or 'Từ chối'.";

                var notificationRecipient = await _context.NotificationRecipients
                    .Include(nr => nr.Notification)
                        .ThenInclude(n => n.Emergency)
                    .FirstOrDefaultAsync(nr => nr.NotificationId == notificationId && nr.Username == username);

                if (notificationRecipient == null)
                    return "Notification recipient not found.";

                if (notificationRecipient.ResponseStatus != "Chưa phản hồi")
                    return "Notification has already been responded to.";

                // Ràng buộc ngày ResponseGo
                if (responseGo.HasValue)
                {
                    var notificationDate = notificationRecipient.Notification?.NotificationDate;
                    var endDate = notificationRecipient.Notification?.Emergency?.EndDate;
                    if (notificationDate.HasValue && responseGo < notificationDate)
                        return "ResponseGo cannot be before the notification date.";
                    if (endDate.HasValue && responseGo > endDate)
                        return "ResponseGo cannot be after the end date of the emergency.";
                }

                notificationRecipient.ResponseStatus = responseStatus;
                notificationRecipient.ResponseDate = DateTime.Now;
                notificationRecipient.ResponseGo = responseGo;
                notificationRecipient.ResponseTime = responseTime;

                await _context.SaveChangesAsync();
                return "Notification response updated successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating notification response");
                throw;
            }
        }

        public async Task<string> CreateAdminNotificationForNewEmergency(int emergencyId, string createdBy)
        {
            var emergency = await _context.Emergencies
                .Include(e => e.Hospital)
                .FirstOrDefaultAsync(e => e.EmergencyId == emergencyId);
            if (emergency == null)
                return "Emergency not found.";

            // Tạo notification cho staff và admin
            var notification = new Notification
            {
                EmergencyId = emergencyId,
                NotificationStatus = "Đã gửi",
                NotificationTitle = $"Đơn khẩn cấp mới từ người dùng {createdBy}",
                NotificationContent = $"Người dùng {createdBy} vừa tạo đơn khẩn cấp cần {emergency.RequiredUnits} đơn vị nhóm máu {emergency.BloodType} tại {emergency.Hospital?.HospitalName}",
                NotificationDate = DateOnly.FromDateTime(DateTime.Now)
            };
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // Lấy tất cả staff và admin
            var staffAndAdmins = await _context.Users
                .Where(u => u.Role == "Staff" || u.Role == "Admin")
                .ToListAsync();
            foreach (var user in staffAndAdmins)
            {
                var recipient = new NotificationRecipient
                {
                    NotificationId = notification.NotificationId,
                    Username = user.Username,
                    ResponseStatus = null,
                    ResponseDate = null,
                    ResponseGo = null,
                    ResponseTime = null
                };
                _context.NotificationRecipients.Add(recipient);
            }
            await _context.SaveChangesAsync();
            return "Admin notification for new emergency created successfully.";
        }

        public async Task<string> CreateUserNotificationBloodTransferring(int emergencyId)
        {
            var emergency = await _context.Emergencies
                .Include(e => e.Hospital)
                .FirstOrDefaultAsync(e => e.EmergencyId == emergencyId);
            if (emergency == null)
                return "Emergency not found.";
            if (string.IsNullOrEmpty(emergency.Username))
                return "Emergency creator not found.";

            var notification = new Notification
            {
                EmergencyId = emergencyId,
                NotificationStatus = "Đã gửi",
                NotificationTitle = $"Đơn khẩn cấp của bạn đã được xử lý",
                NotificationContent = $"Đơn khẩn cấp của bạn đã được chấp thuận và lượng máu đang được chuyển đến tại {emergency.Hospital.HospitalName}",
                NotificationDate = DateOnly.FromDateTime(DateTime.Now)
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            var recipient = new NotificationRecipient
            {
                NotificationId = notification.NotificationId,
                Username = emergency.Username,
                ResponseStatus = "Chưa phản hồi",
                ResponseDate = null,
                ResponseGo = null,
                ResponseTime = null
            };
            _context.NotificationRecipients.Add(recipient);
            await _context.SaveChangesAsync();

            return "User notification created successfully.";
        }

        public async Task<string> CreateUserNotificationApprovedButInsufficientBlood(int emergencyId)
        {
            var emergency = await _context.Emergencies
                .Include(e => e.Hospital)
                .FirstOrDefaultAsync(e => e.EmergencyId == emergencyId);
            if (emergency == null)
                return "Emergency not found.";
            if (string.IsNullOrEmpty(emergency.Username))
                return "Emergency creator not found.";

            var notification = new Notification
            {
                EmergencyId = emergencyId,
                NotificationStatus = "Đã gửi",
                NotificationTitle = "Đơn khẩn cấp đã được xét duyệt",
                NotificationContent = "Đơn khẩn cấp của bạn đã được xét duyệt nhưng vì lượng máu bạn yêu cầu không đủ hoặc khoảng cách vận chuyển máu xa nên chúng tôi đã gửi thông báo đến các người hiến tặng có cùng nhóm máu mà bạn yêu cầu và cùng tỉnh thành nơi bạn đang cư trú.",
                NotificationDate = DateOnly.FromDateTime(DateTime.Now)
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            var recipient = new NotificationRecipient
            {
                NotificationId = notification.NotificationId,
                Username = emergency.Username,
                ResponseStatus = "Chưa phản hồi",
                ResponseDate = null,
                ResponseGo = null,
                ResponseTime = null
            };
            _context.NotificationRecipients.Add(recipient);
            await _context.SaveChangesAsync();

            return "User notification for approved but insufficient blood created successfully.";
        }
    }
} 