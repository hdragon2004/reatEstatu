using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RealEstateHubAPI.Hubs;
using RealEstateHubAPI.Model;
using RealEstateHubAPI.Models;
using RealEstateHubAPI.Utils;
using System.Security.Claims;

namespace RealEstateHubAPI.Services
{
    /// <summary>
    /// Service tập trung để quản lý việc tạo và gửi thông báo real-time
    /// Đảm bảo mọi thông báo đều được gửi kèm banner real-time đến user
    /// </summary>
    public interface INotificationService
    {
        Task CreateAndSendNotificationAsync(Notification notification);
        Task CreateAndSendNotificationAsync(int userId, string title, string message, string type,
            int? postId = null, int? savedSearchId = null, int? appointmentId = null, int? messageId = null, int? senderId = null);
    }

    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(
            ApplicationDbContext context,
            IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        /// <summary>
        /// Tạo và gửi thông báo real-time với banner
        /// </summary>
        public async Task CreateAndSendNotificationAsync(Notification notification)
        {
            // Set timestamp if not provided
            if (notification.CreatedAt == default)
            {
                notification.CreatedAt = DateTimeHelper.GetVietnamNow();
            }

            // Save to database
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // Send real-time notification via SignalR
            await SendRealTimeNotificationAsync(notification);
        }

        /// <summary>
        /// Tạo thông báo từ parameters và gửi real-time
        /// </summary>
        public async Task CreateAndSendNotificationAsync(int userId, string title, string message, string type,
            int? postId = null, int? savedSearchId = null, int? appointmentId = null, int? messageId = null, int? senderId = null)
        {
            var notification = new Notification
            {
                UserId = userId,
                PostId = postId,
                SavedSearchId = savedSearchId,
                AppointmentId = appointmentId,
                MessageId = messageId,
                Title = title,
                Message = message,
                Type = type,
                IsRead = false,
                CreatedAt = DateTimeHelper.GetVietnamNow(),
                SenderId = senderId
            };

            await CreateAndSendNotificationAsync(notification);
        }

        /// <summary>
        /// Gửi thông báo real-time qua SignalR để hiển thị banner
        /// </summary>
        private async Task SendRealTimeNotificationAsync(Notification notification)
        {
            try
            {
                var groupName = $"user_{notification.UserId}";
                Console.WriteLine($"[DEBUG] Sending notification {notification.Id} (Type: {notification.Type}) to group: {groupName}");

                // Gửi đến user cụ thể qua SignalR group "user_{id}" để hiển thị banner real-time
                await _hubContext.Clients.Group(groupName).SendAsync("ReceiveNotification", new
                {
                    Id = notification.Id,
                    UserId = notification.UserId,
                    PostId = notification.PostId,
                    SavedSearchId = notification.SavedSearchId,
                    AppointmentId = notification.AppointmentId,
                    MessageId = notification.MessageId,
                    SenderId = notification.SenderId,
                    Title = notification.Title,
                    Message = notification.Message,
                    Type = notification.Type,
                    CreatedAt = notification.CreatedAt,
                    IsRead = notification.IsRead
                });

                Console.WriteLine($"[DEBUG] Successfully sent notification {notification.Id} to group: {groupName}");
            }
            catch (Exception ex)
            {
                // Log error but don't throw - notification should still be saved
                Console.WriteLine($"[ERROR] Failed to send real-time notification {notification.Id}: {ex.Message}");
            }
        }
    }
}
