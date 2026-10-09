using System.Collections.Generic;
using System.Threading.Tasks;
using SkillBridge.Models;

namespace SkillBridge.Services
{
    public interface IChatService
    {
        Task<IEnumerable<DisputeChatMessage>> GetMessagesAsync(int disputeId);
        Task<DisputeChatMessage?> SaveMessageAsync(int disputeId, int senderUserId, string senderRole, string text);
    }
}
