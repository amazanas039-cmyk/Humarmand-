using System.Collections.Generic;
using System.Threading.Tasks;
using SkillBridge.Models;

namespace SkillBridge.Services
{
    public interface IDisputeService
    {
        Task<int> RaiseDisputeAsync(int requestId, int userId, string reason);
        Task<bool> ResolveAsync(int disputeId, string resType, string resText, int adminUserId);
        Task<IEnumerable<Dispute>> GetOpenAsync();
        Task<Dispute?> GetByIdAsync(int id);
        Task<IEnumerable<Dispute>> GetForUserAsync(int userId);
    }
}
