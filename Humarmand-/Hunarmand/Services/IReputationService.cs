using System.Threading.Tasks;

namespace Hunarmand.Services
{
    public interface IReputationService
    {
        Task RecalculateAsync(int labourerId);
    }
}
