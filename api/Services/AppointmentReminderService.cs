using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RealEstateHubAPI.Hubs;
using RealEstateHubAPI.Model;
using RealEstateHubAPI.Models;
using RealEstateHubAPI.Services;
using RealEstateHubAPI.Utils;

namespace RealEstateHubAPI.Services
{

    public class AppointmentReminderService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AppointmentReminderService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1); // Chạy mỗi 1 phút

        public AppointmentReminderService(
            IServiceProvider serviceProvider,
            ILogger<AppointmentReminderService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("AppointmentReminderService started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckAndCreateRemindersAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in AppointmentReminderService");
                }

                // Đợi 1 phút trước khi check lại
                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("AppointmentReminderService stopped");
        }
        private async Task CheckAndCreateRemindersAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var appointmentService = scope.ServiceProvider.GetRequiredService<IAppointmentService>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            try
            {
                // Lấy các Appointment đã đến thời điểm nhắc
                var dueAppointments = await appointmentService.GetDueAppointmentsAsync();

                if (!dueAppointments.Any())
                {
                    return; // Không có appointment nào cần nhắc
                }

                _logger.LogInformation($"Found {dueAppointments.Count()} appointments due for reminder");

                foreach (var appointment in dueAppointments)
                {
                    try
                    {
                        // Gửi notification real-time qua NotificationService (tự tạo và lưu Notification)
                        await notificationService.CreateAndSendNotificationAsync(
                            appointment.UserId,
                            "Nhắc lịch hẹn",
                            $"Bạn có lịch hẹn '{appointment.Title}' vào lúc {appointment.AppointmentTime:dd/MM/yyyy HH:mm}",
                            "Reminder",
                            postId: appointment.PostId,
                            appointmentId: appointment.Id
                        );

                        // Đánh dấu appointment đã được nhắc
                        appointment.IsNotified = true;

                        _logger.LogInformation(
                            $"Created reminder notification for Appointment {appointment.Id}, User {appointment.UserId}, " +
                            $"AppointmentTime: {appointment.AppointmentTime:yyyy-MM-dd HH:mm}");

                        // TODO: Tích hợp Firebase Cloud Messaging (FCM) sau này
                        // await _fcmService.SendNotificationAsync(appointment.UserId, notification);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error creating reminder for Appointment {appointment.Id}");
                        // Tiếp tục xử lý các appointment khác
                    }
                }

                // Lưu tất cả thay đổi
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CheckAndCreateRemindersAsync");
            }
        }
    }
}

