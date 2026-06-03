using System.Collections.Generic;
using System.Threading.Tasks;
using SkillBridge.Models;

namespace SkillBridge.Services
{
    public interface IAdminService
    {
        Task<IEnumerable<User>> GetAllUsersAsync(string? search);
        Task SuspendUserAsync(int userId, bool suspend);
    }
}
