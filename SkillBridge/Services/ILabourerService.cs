using System.Collections.Generic;
using System.Threading.Tasks;
using SkillBridge.Models;

namespace SkillBridge.Services
{
    public interface ILabourerService
    {
        Task<IEnumerable<Labourer>> GetPendingVerificationAsync();
        Task ApproveAsync(int labourerId, string note);
        Task RejectAsync(int labourerId, string note);
        Task<IEnumerable<Labourer>> SearchNearbyAsync(double lat, double lng, double radiusKm, int? categoryId, decimal? minRating, int? minExp);
        Task<(Labourer Profile, List<Review> Reviews)?> GetProfileAsync(int labourerId);
        Task UpdateAvailabilityAsync(int labourerId, string scheduleJson);
        Task UpdateProfileAsync(Labourer l);
    }
}
