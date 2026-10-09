using System.Collections.Generic;
using System.Threading.Tasks;
using Hunarmand.Models;

namespace Hunarmand.Services
{
    public interface INotificationService
    {
        Task<int> CreateAsync(int userId, string message, string type);
        Task<IEnumerable<Notification>> GetUnreadAsync(int userId);
        Task MarkReadAsync(int notificationId);
        Task MarkAllReadAsync(int userId);
        Task<int> GetUnreadCountAsync(int userId);
    }
}
