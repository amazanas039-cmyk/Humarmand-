using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Hunarmand.Models;

namespace Hunarmand.Services
{
    public class MatchingService : IMatchingService
    {
        private readonly ILogger<MatchingService> _logger;

        // Weight constants for the SmartScore algorithm
        private const double W_REPUTATION = 0.40;
        private const double W_DISTANCE = 0.30;
        private const double W_COMPLETION = 0.20;
        private const double W_EXPERIENCE = 0.10;

        public MatchingService(ILogger<MatchingService> logger)
        {
            _logger = logger;
        }

        public IEnumerable<Labourer> RankAndSort(IEnumerable<Labourer> labourers, double? customerLat, double? customerLng)
        {
            var list = labourers.ToList();
            double maxDistance = list.Max(l => l.DistanceKm ?? 0) + 0.01; // avoid div-by-zero

            foreach (var l in list)
            {
                double reputationNorm = Math.Min((double)l.ReputationScore / 5.0, 1.0);
                double distanceNorm = 1.0 - Math.Min((l.DistanceKm ?? maxDistance) / maxDistance, 1.0);
                double completionNorm = Math.Min((double)l.CompletionRate / 100.0, 1.0);
                double experienceNorm = Math.Min(l.ExperienceYears / 20.0, 1.0);

                l.SmartScore = Math.Round(
                    (W_REPUTATION * reputationNorm) +
                    (W_DISTANCE * distanceNorm) +
                    (W_COMPLETION * completionNorm) +
                    (W_EXPERIENCE * experienceNorm),
                    4);
            }

            _logger.LogInformation("Ranked {Count} labourers by SmartScore", list.Count);
            return list.OrderByDescending(l => l.SmartScore);
        }
    }
}
