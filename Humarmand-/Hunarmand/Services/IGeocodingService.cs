using System.Threading.Tasks;

namespace Hunarmand.Services
{
    public interface IGeocodingService
    {
        Task<(double lat, double lng)?> GeocodeAsync(string address);
    }
}
