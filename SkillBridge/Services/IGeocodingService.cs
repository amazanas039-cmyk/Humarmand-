using System.Threading.Tasks;

namespace SkillBridge.Services
{
    public interface IGeocodingService
    {
        Task<(double lat, double lng)?> GeocodeAsync(string address);
    }
}
