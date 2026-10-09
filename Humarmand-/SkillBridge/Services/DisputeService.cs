using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SkillBridge.Models;
using SkillBridge.Repositories;

namespace SkillBridge.Services
{
    public class DisputeService : IDisputeService
    {
        private readonly DisputeRepository _disputeRepo;
        private readonly INotificationService _notificationService;
        private readonly ILogger<DisputeService> _logger;

        public DisputeService(
            DisputeRepository disputeRepo,
            INotificationService notificationService,
            ILogger<DisputeService> logger)
        {
            _disputeRepo = disputeRepo;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<int> RaiseDisputeAsync(int requestId, int userId, string reason)
        {
            int disputeId = await _disputeRepo.Raise(requestId, userId, reason);
            _logger.LogInformation("Dispute {DID} raised by user {UID} for request {RID}", disputeId, userId, requestId);

            // Notify admins — for now, notify the raising user of confirmation
            await _notificationService.CreateAsync(
                userId,
                $"Your dispute #{disputeId} for job #{requestId} has been submitted and is under review.",
                "Dispute");

            return disputeId;
        }

        public async Task<bool> ResolveAsync(int disputeId, string resType, string resText, int adminUserId)
        {
            var dispute = await _disputeRepo.GetById(disputeId);
            if (dispute == null)
            {
                _logger.LogWarning("ResolveDispute failed: dispute {DID} not found", disputeId);
                return false;
            }

            await _disputeRepo.Resolve(disputeId, resType, resText);
            _logger.LogInformation("Dispute {DID} resolved by admin {AUID}: {ResType}", disputeId, adminUserId, resType);

            // Notify the person who raised the dispute
            await _notificationService.CreateAsync(
                dispute.RaisedByUserID,
                $"Your dispute #{disputeId} has been resolved. Resolution: {resText}",
                "Dispute");

            return true;
        }

        public async Task<IEnumerable<Dispute>> GetOpenAsync()
        {
            return await _disputeRepo.GetOpen();
        }

        public async Task<Dispute?> GetByIdAsync(int id)
        {
            return await _disputeRepo.GetById(id);
        }

        public async Task<IEnumerable<Dispute>> GetForUserAsync(int userId)
        {
            return await _disputeRepo.GetForUser(userId);
        }
    }
}
