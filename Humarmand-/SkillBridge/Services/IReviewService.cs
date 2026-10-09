using System.Collections.Generic;
using System.Threading.Tasks;
using SkillBridge.Models;

namespace SkillBridge.Services
{
    public interface IReviewService
    {
        Task SubmitReviewAsync(int requestId, int reviewerUserId, int targetLabourerId, int rating, string comment);
        Task<IEnumerable<Review>> GetForLabourerAsync(int labourerId);
        Task<bool> HasReviewedAsync(int requestId, int userId);
    }
}
