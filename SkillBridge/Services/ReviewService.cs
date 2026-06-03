using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SkillBridge.Models;
using SkillBridge.Repositories;

namespace SkillBridge.Services
{
    public class ReviewService : IReviewService
    {
        private readonly ReviewRepository _reviewRepo;
        private readonly IReputationService _reputationService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<ReviewService> _logger;

        public ReviewService(
            ReviewRepository reviewRepo,
            IReputationService reputationService,
            INotificationService notificationService,
            ILogger<ReviewService> logger)
        {
            _reviewRepo = reviewRepo;
            _reputationService = reputationService;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task SubmitReviewAsync(int requestId, int reviewerUserId, int targetLabourerId, int rating, string comment)
        {
            // Check if already reviewed
            bool alreadyReviewed = await _reviewRepo.HasReviewed(requestId, reviewerUserId);
            if (alreadyReviewed)
            {
                throw new InvalidOperationException("You have already reviewed this job.");
            }

            // Validate rating
            if (rating < 1 || rating > 5)
            {
                throw new ArgumentOutOfRangeException(nameof(rating), "Rating must be between 1 and 5.");
            }

            await _reviewRepo.Create(requestId, reviewerUserId, targetLabourerId, rating, comment);
            _logger.LogInformation("Review submitted for request {RID} by user {UID}: {Rating}★",
                requestId, reviewerUserId, rating);

            // Recalculate reputation
            await _reputationService.RecalculateAsync(targetLabourerId);

            // Notify the labourer
            await _notificationService.CreateAsync(
                targetLabourerId,
                $"You received a new {rating}★ review for job #{requestId}.",
                "Review");
        }

        public async Task<IEnumerable<Review>> GetForLabourerAsync(int labourerId)
        {
            return await _reviewRepo.GetForLabourer(labourerId);
        }

        public async Task<bool> HasReviewedAsync(int requestId, int userId)
        {
            return await _reviewRepo.HasReviewed(requestId, userId);
        }
    }
}
