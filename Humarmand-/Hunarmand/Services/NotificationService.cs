using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Hunarmand.Models;
using Hunarmand.Repositories;

namespace Hunarmand.Services
{
    public class NotificationService : INotificationService
    {
        private readonly NotificationRepository _notificationRepo;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(NotificationRepository notificationRepo, ILogger<NotificationService> logger)
        {
            _notificationRepo = notificationRepo;
            _logger = logger;
        }

        public async Task<int> CreateAsync(int userId, string message, string type)
        {
            int id = await _notificationRepo.Create(userId, message, type);
            _logger.LogDebug("Notification {NID} created for user {UID}: {Type}", id, userId, type);
            return id;
        }

        public async Task<IEnumerable<Notification>> GetUnreadAsync(int userId)
        {
            return await _notificationRepo.GetUnread(userId);
        }

        public async Task MarkReadAsync(int notificationId)
        {
            await _notificationRepo.MarkRead(notificationId);
        }

        public async Task MarkAllReadAsync(int userId)
        {
            await _notificationRepo.MarkAllRead(userId);
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _notificationRepo.GetUnreadCount(userId);
        }
    }
}
