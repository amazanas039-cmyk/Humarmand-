using System.Threading.Tasks;
using SkillBridge.Models;

namespace SkillBridge.Services
{
    public interface IUserService
    {
        Task<User?> LoginAsync(string email, string password, string expectedRole);
        Task<bool> RegisterCustomerAsync(Customer c, string password);
        Task<bool> RegisterLabourerAsync(Labourer l, string password);
        Task<User?> GetByIdAsync(int userId);
        Task<int?> GetCustomerIdByUserIdAsync(int userId);
        Task<int?> GetLabourerIdByUserIdAsync(int userId);
        Task<int?> GetAdminIdByUserIdAsync(int userId);
    }
}
