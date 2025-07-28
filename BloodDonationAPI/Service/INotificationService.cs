using BloodDonationAPI.DTO;
using BloodDonationAPI.Entities;

namespace BloodDonationAPI.Service
{
    public interface INotificationService
    {
        Task<List<NotificationDto>> GetNotifications();
        Task<string> CreateNotificationForEmergency(int emergencyId);
        Task<List<NotificationDto>> GetNotificationsByBloodType(string bloodType);
        Task<List<NotificationDto>> GetUserNotifications(string username);
        Task<string> UpdateNotificationResponse(int notificationId, string username, string dtoResponseStatus, DateOnly? dtoResponseGo, TimeOnly? dtoResponseTime);
        Task<string> CreateAdminNotificationForNewEmergency(int emergencyId, string createdBy);
        Task<string> CreateUserNotificationBloodTransferring(int emergencyId);
        Task<string> CreateUserNotificationApprovedButInsufficientBlood(int emergencyId);
    }
} 