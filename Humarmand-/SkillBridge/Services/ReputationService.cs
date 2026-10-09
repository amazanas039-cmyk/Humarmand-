using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SkillBridge.Repositories;

namespace SkillBridge.Services
{
    public class ReputationService : IReputationService
    {
        private readonly ReviewRepository _reviewRepo;
        private readonly ILogger<ReputationService> _logger;

        public ReputationService(ReviewRepository reviewRepo, ILogger<ReputationService> logger)
        {
            _reviewRepo = reviewRepo;
            _logger = logger;
        }

        /// <summary>
        /// Recalculates a labourer's reputation score based on all their reviews.
        /// The stored procedure sp_RecalculateReputation handles the DB update.
        /// For now, this method logs the intent; the actual recalculation
        /// is triggered by the review creation stored procedure in the DB.
        /// </summary>
        public async Task RecalculateAsync(int labourerId)
        {
            var reviews = await _reviewRepo.GetForLabourer(labourerId);
            var reviewList = reviews.ToList();

            if (reviewList.Count == 0)
            {
                _logger.LogInformation("No reviews for labourer {LID}, reputation unchanged", labourerId);
                return;
            }

            double avg = reviewList.Average(r => r.StarRating);
            _logger.LogInformation("Labourer {LID} reputation recalculated: {Avg:F2} from {Count} reviews",
                labourerId, avg, reviewList.Count);
            // Note: The stored procedure sp_CreateReview auto-updates the reputation in the DB.
        }
    }
}
