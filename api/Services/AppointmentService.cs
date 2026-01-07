using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RealEstateHubAPI.DTOs;
using RealEstateHubAPI.Hubs;
using RealEstateHubAPI.Model;
using RealEstateHubAPI.Models;
using RealEstateHubAPI.Utils;
using RealEstateHubAPI.Services;

namespace RealEstateHubAPI.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AppointmentService> _logger;
        private readonly IHubContext<NotificationHub>? _notificationHub;
        private readonly INotificationService _notificationService;

        public AppointmentService(
            ApplicationDbContext context,
            ILogger<AppointmentService> logger,
            INotificationService notificationService,
            IHubContext<NotificationHub>? notificationHub = null)
        {
            _context = context;
            _logger = logger;
            _notificationService = notificationService;
            _notificationHub = notificationHub;
        }

        public async Task<AppointmentDto> CreateAppointmentAsync(int userId, CreateAppointmentDto dto)
        {
            // Validate: AppointmentTime phải trong tương lai
            // Frontend gửi local time, cần so sánh với local time hiện tại
            var now = DateTimeHelper.GetVietnamNow(); // Vietnam local time để so sánh với local time từ frontend
            if (dto.AppointmentTime <= now)
            {
                throw new ArgumentException("AppointmentTime must be in the future");
            }

            // Validate: PostId phải tồn tại
            var postExists = await _context.Posts.AnyAsync(p => p.Id == dto.PostId);
            if (!postExists)
            {
                throw new ArgumentException($"Post with Id {dto.PostId} does not exist");
            }

            // Lấy thông tin post để lấy UserId của chủ bài post
            var post = await _context.Posts
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == dto.PostId);

            if (post == null)
            {
                throw new ArgumentException($"Post with Id {dto.PostId} does not exist");
            }
            
            // Nếu user cố gắng tạo lịch hẹn cho chính mình (owner của post), chặn lại
            if (post.UserId == userId)
            {
                throw new ArgumentException("Không thể tạo lịch hẹn cho chính bạn trên bài đăng của chính bạn");
            }

            var appointment = new Appointment
            {
                UserId = userId,
                PostId = dto.PostId,
                Title = dto.Title,
                Description = dto.Description,
                AppointmentTime = dto.AppointmentTime,
                ReminderMinutes = dto.ReminderMinutes,
                IsNotified = false,
                Status = AppointmentStatus.PENDING, // Mặc định là PENDING khi tạo mới
                CreatedAt = DateTimeHelper.GetVietnamNow()
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Created Appointment {appointment.Id} for User {userId}, AppointmentTime: {appointment.AppointmentTime}");

            // Gửi thông báo cho chủ bài post (post.UserId) để chấp nhận lịch hẹn
            var postOwnerId = post.UserId;
            Console.WriteLine($"[DEBUG] Creating appointment {appointment.Id} - Creator: {userId}, PostOwner: {postOwnerId}");

            // Nếu người tạo lịch hẹn chính là chủ bài (ví dụ user tự tạo cho chính mình), không cần gửi notification cho chính họ
            if (postOwnerId != userId)
            {
                // Lấy thông tin người tạo lịch hẹn (user A)
                var appointmentCreator = await _context.Users.FindAsync(userId);

                // Tạo và gửi thông báo real-time qua service tập trung
                var creatorName = appointmentCreator?.Name ?? "người dùng";
                var requestMessage = $"Bạn có yêu cầu lịch hẹn từ {creatorName} cho bài viết '{post.Title}' vào lúc {dto.AppointmentTime:dd/MM/yyyy HH:mm}. Vui lòng chấp nhận hoặc từ chối.";
                Console.WriteLine($"[DEBUG] Sending AppointmentRequest notification to postOwner {postOwnerId}");

                await _notificationService.CreateAndSendNotificationAsync(
                    postOwnerId, // Gửi cho chủ bài post (user B)
                    "Yêu cầu lịch hẹn mới",
                    requestMessage,
                    "AppointmentRequest",
                    postId: dto.PostId,
                    appointmentId: appointment.Id,
                    messageId: null,
                    senderId: userId
                );
            }
            else
            {
                Console.WriteLine($"[DEBUG] Skipping notification - creator {userId} is same as postOwner {postOwnerId}");
            }

            return MapToDto(appointment);
        }

        public async Task<IEnumerable<AppointmentDto>> GetUserAppointmentsAsync(int userId)
        {
            // Trả về TẤT CẢ appointments của user, bao gồm cả những cái bị hủy/từ chối
            // để có thể hiển thị trong tab "Bị từ chối"
            var appointments = await _context.Appointments
                .Where(a => a.UserId == userId)
                .OrderBy(a => a.AppointmentTime)
                .ToListAsync();

            return appointments.Select(MapToDto);
        }

        public async Task<IEnumerable<AppointmentDto>> GetPendingAppointmentsForPostOwnerAsync(int postOwnerId)
        {
            // Lấy các appointments chưa được chấp nhận (PENDING) cho các bài post của user này
            var appointments = await _context.Appointments
                .Include(a => a.Post)
                .Where(a => a.Post != null && 
                           a.Post.UserId == postOwnerId && 
                           a.Status == AppointmentStatus.PENDING)
                .OrderBy(a => a.CreatedAt)
                .ToListAsync();

            return appointments.Select(MapToDto);
        }

        public async Task<IEnumerable<AppointmentDto>> GetAllAppointmentsForPostOwnerAsync(int postOwnerId)
        {
            // Lấy TẤT CẢ appointments (PENDING, ACCEPTED, REJECTED) cho các bài post của user này
            var appointments = await _context.Appointments
                .Include(a => a.Post)
                .Where(a => a.Post != null && 
                           a.Post.UserId == postOwnerId)
                .OrderBy(a => a.AppointmentTime)
                .ToListAsync();

            return appointments.Select(MapToDto);
        }

        public async Task<bool> CancelAppointmentAsync(int appointmentId, int userId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Post)
                .FirstOrDefaultAsync(a => a.Id == appointmentId && a.UserId == userId);

            if (appointment == null)
            {
                return false;
            }

            appointment.Status = AppointmentStatus.REJECTED;
            await _context.SaveChangesAsync();

            // Gửi thông báo cho post owner (user B) rằng lịch hẹn đã bị hủy (kèm senderId)
            await _notificationService.CreateAndSendNotificationAsync(
                appointment.Post.UserId, // Gửi cho user B (chủ bài đăng)
                "Lịch hẹn đã bị hủy",
                $"Lịch hẹn '{appointment.Title}' vào lúc {appointment.AppointmentTime:dd/MM/yyyy HH:mm} đã bị hủy bởi người tạo.",
                "AppointmentRejected",
                postId: appointment.PostId,
                appointmentId: appointment.Id,
                senderId: userId
            );

            _logger.LogInformation($"Canceled Appointment {appointmentId} for User {userId}");

            return true;
        }

        public async Task<bool> ConfirmAppointmentAsync(int appointmentId, int callerUserId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Post)
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == appointmentId &&
                                         a.Status == AppointmentStatus.PENDING);

            if (appointment == null)
            {
                return false;
            }

            // Chỉ post owner mới có quyền confirm appointment
            if (appointment.Post.UserId != callerUserId)
            {
                _logger.LogWarning($"User {callerUserId} tried to confirm appointment {appointmentId} but is not the post owner");
                return false;
            }

            appointment.Status = AppointmentStatus.ACCEPTED;
            await _context.SaveChangesAsync();

            // Gửi thông báo cho user đã tạo appointment (appointment.UserId) rằng lịch hẹn đã được chấp nhận
            Console.WriteLine($"[DEBUG] Sending AppointmentConfirmed notification to appointment creator {appointment.UserId}");

            await _notificationService.CreateAndSendNotificationAsync(
                appointment.UserId, // Gửi cho user A (người tạo lịch hẹn)
                "Lịch hẹn đã được chấp nhận",
                $"Lịch hẹn '{appointment.Title}' vào lúc {appointment.AppointmentTime:dd/MM/yyyy HH:mm} đã được chấp nhận.",
                "AppointmentConfirmed",
                postId: appointment.PostId,
                appointmentId: appointment.Id,
                senderId: callerUserId
            );

            _logger.LogInformation($"Confirmed Appointment {appointmentId} by PostOwner {callerUserId}");

            return true;
        }

        public async Task<bool> RejectAppointmentAsync(int appointmentId, int callerUserId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Post)
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == appointmentId &&
                                         a.Status == AppointmentStatus.PENDING);

            if (appointment == null)
            {
                return false;
            }

            // Cho phép cả post owner và appointment creator reject/cancel appointment
            if (appointment.Post.UserId != callerUserId && appointment.UserId != callerUserId)
            {
                _logger.LogWarning($"User {callerUserId} tried to reject appointment {appointmentId} but is not authorized");
                return false;
            }

            appointment.Status = AppointmentStatus.REJECTED;
            await _context.SaveChangesAsync();

            // Luôn gửi thông báo cho appointment creator (user A), bất kể ai gọi API reject
            Console.WriteLine($"[DEBUG] Sending AppointmentRejected notification to appointment creator {appointment.UserId}");

            await _notificationService.CreateAndSendNotificationAsync(
                appointment.UserId, // Luôn gửi cho user A (người tạo lịch hẹn)
                "Lịch hẹn đã bị từ chối",
                $"Lịch hẹn '{appointment.Title}' vào lúc {appointment.AppointmentTime:dd/MM/yyyy HH:mm} đã bị từ chối.",
                "AppointmentRejected",
                postId: appointment.PostId,
                appointmentId: appointment.Id,
                senderId: callerUserId
            );

            _logger.LogInformation($"Rejected Appointment {appointmentId} by User {callerUserId}");

            return true;
        }

        public async Task<IEnumerable<Appointment>> GetDueAppointmentsAsync()
        {
            // Sử dụng Vietnam time để so sánh với AppointmentTime (cũng là Vietnam time)
            var now = DateTimeHelper.GetVietnamNow();

            // Chỉ gửi reminder cho appointments đã được chấp nhận (ACCEPTED)
            var dueAppointments = await _context.Appointments
                .Where(a => !a.IsNotified &&
                           a.Status == AppointmentStatus.ACCEPTED && // CHỈ gửi reminder khi đã được chấp nhận
                           a.AppointmentTime.AddMinutes(-a.ReminderMinutes) <= now)
                .ToListAsync();

            return dueAppointments;
        }

        private AppointmentDto MapToDto(Appointment appointment)
        {
            return new AppointmentDto
            {
                Id = appointment.Id,
                UserId = appointment.UserId,
                PostId = appointment.PostId,
                Title = appointment.Title,
                Description = appointment.Description,
                AppointmentTime = appointment.AppointmentTime,
                ReminderMinutes = appointment.ReminderMinutes,
                IsNotified = appointment.IsNotified,
                Status = appointment.Status,
                CreatedAt = appointment.CreatedAt
            };
        }
    }
}

