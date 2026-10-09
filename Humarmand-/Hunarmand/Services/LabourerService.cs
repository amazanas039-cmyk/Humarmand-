using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Hunarmand.Models;
using Hunarmand.Repositories;

namespace Hunarmand.Services
{
    public class LabourerService : ILabourerService
    {
        private readonly LabourerRepository _labourerRepo;
        private readonly INotificationService _notificationService;
        private readonly ILogger<LabourerService> _logger;

        public LabourerService(LabourerRepository labourerRepo, INotificationService notificationService, ILogger<LabourerService> logger)
        {
            _labourerRepo = labourerRepo;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<IEnumerable<Labourer>> GetPendingVerificationAsync()
        {
            return await _labourerRepo.GetPendingVerification();
        }

        public async Task ApproveAsync(int labourerId, string note)
        {
            await _labourerRepo.Approve(labourerId, note);
            _logger.LogInformation("Labourer {LID} approved", labourerId);

            // Get the profile to find user ID for notification
            var profileResult = await _labourerRepo.GetProfile(labourerId);
            if (profileResult.HasValue)
            {
                await _notificationService.CreateAsync(
                    profileResult.Value.Profile.UserID,
                    "Congratulations! Your profile has been verified and is now live.",
                    "Verification");
            }
        }

        public async Task RejectAsync(int labourerId, string note)
        {
            await _labourerRepo.Reject(labourerId, note);
            _logger.LogInformation("Labourer {LID} rejected. Note: {Note}", labourerId, note);

            var profileResult = await _labourerRepo.GetProfile(labourerId);
            if (profileResult.HasValue)
            {
                await _notificationService.CreateAsync(
                    profileResult.Value.Profile.UserID,
                    $"Your profile verification was not approved. Reason: {note}",
                    "Verification");
            }
        }

        public async Task<IEnumerable<Labourer>> SearchNearbyAsync(double lat, double lng, double radiusKm, int? categoryId, decimal? minRating, int? minExp)
        {
            return await _labourerRepo.SearchNearby(lat, lng, radiusKm, categoryId, minRating, minExp);
        }

        public async Task<(Labourer Profile, List<Review> Reviews)?> GetProfileAsync(int labourerId)
        {
            return await _labourerRepo.GetProfile(labourerId);
        }

        public async Task UpdateAvailabilityAsync(int labourerId, string scheduleJson)
        {
            await _labourerRepo.UpdateAvailability(labourerId, scheduleJson);
            _logger.LogInformation("Labourer {LID} availability updated", labourerId);
        }

        public async Task UpdateProfileAsync(Labourer l)
        {
            await _labourerRepo.UpdateProfile(l);
            _logger.LogInformation("Labourer {LID} profile updated", l.LabourerID);
        }
    }
}
