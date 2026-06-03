using System.Threading.Tasks;

namespace SkillBridge.Services
{
    public interface IReputationService
    {
        Task RecalculateAsync(int labourerId);
    }
}
