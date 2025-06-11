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
                return await _context.Notifications
                    .Include(n => n.Emergency)
                        .ThenInclude(e => e.Hospital)
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
                _logger.LogError(ex, "Error getting notifications");
                throw;
            }
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

                // Check if notification already exists
                var existingNotification = await _context.Notifications
                    .FirstOrDefaultAsync(n => n.EmergencyId == emergencyId);

                if (existingNotification != null)
                    return "Notification already exists for this emergency.";

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

                // Create notification recipients for users with matching blood type
                var matchingUsers = await _context.Users
                    .Where(u => u.BloodType == emergency.BloodType && u.ProfileStatus == "Active")
                    .ToListAsync();

                foreach (var user in matchingUsers)
                {
                    var recipient = new NotificationRecipient
                    {
                        NotificationId = notification.NotificationId,
                        Username = user.Username,
                        ResponseStatus = "Chưa phản hồi",
                        ResponseDate = null
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

        public async Task<string> UpdateNotificationResponse(int notificationId, string username, string responseStatus)
        {
            try
            {
                if (string.IsNullOrEmpty(responseStatus))
                    return "Response status is required.";

                if (responseStatus != "Chấp nhận" && responseStatus != "Từ chối")
                    return "Invalid response status. Must be either 'Chấp nhận' or 'Từ chối'.";

                var notificationRecipient = await _context.NotificationRecipients
                    .FirstOrDefaultAsync(nr => nr.NotificationId == notificationId && nr.Username == username);

                if (notificationRecipient == null)
                    return "Notification recipient not found.";

                if (notificationRecipient.ResponseStatus != "Chưa phản hồi")
                    return "Notification has already been responded to.";

                notificationRecipient.ResponseStatus = responseStatus;
                notificationRecipient.ResponseDate = DateTime.Now;

                await _context.SaveChangesAsync();
                return "Notification response updated successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating notification response");
                throw;
            }
        }
    }
} 