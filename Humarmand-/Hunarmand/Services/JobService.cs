using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Hunarmand.Models;
using Hunarmand.Repositories;

namespace Hunarmand.Services
{
    public class JobService : IJobService
    {
        private readonly JobRepository _jobRepo;
        private readonly INotificationService _notificationService;
        private readonly ILogger<JobService> _logger;

        public JobService(JobRepository jobRepo, INotificationService notificationService, ILogger<JobService> logger)
        {
            _jobRepo = jobRepo;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<int> SendHireRequestAsync(JobRequest request)
        {
            // Validate availability first
            bool available = await _jobRepo.CheckAvailability(request.LabourerID, request.PreferredDate, request.PreferredTime);
            if (!available)
            {
                _logger.LogWarning("Hire request rejected: labourer {LID} not available on {Date} at {Time}",
                    request.LabourerID, request.PreferredDate, request.PreferredTime);
                throw new InvalidOperationException("Labourer is not available at the requested time.");
            }

            int requestId = await _jobRepo.Create(request);
            _logger.LogInformation("Job request {RID} created by customer {CID} for labourer {LID}",
                requestId, request.CustomerID, request.LabourerID);

            // Notify the labourer
            await _notificationService.CreateAsync(
                request.LabourerID,
                $"You have a new hire request (#{requestId}). Please review and respond.",
                "NewRequest");

            return requestId;
        }

        public async Task<bool> UpdateStatusAsync(int requestId, string newStatus, string? reason)
        {
            var job = await _jobRepo.GetById(requestId);
            if (job == null)
            {
                _logger.LogWarning("UpdateStatus failed: request {RID} not found", requestId);
                return false;
            }

            await _jobRepo.UpdateStatus(requestId, newStatus, reason);
            _logger.LogInformation("Job request {RID} status changed to {Status}", requestId, newStatus);

            // Notify the other party
            string message = newStatus switch
            {
                "Accepted" => $"Your hire request #{requestId} has been accepted!",
                "Rejected" => $"Your hire request #{requestId} was declined. Reason: {reason ?? "N/A"}",
                "Completed" => $"Job #{requestId} has been marked as completed.",
                "Cancelled" => $"Job #{requestId} has been cancelled.",
                _ => $"Job #{requestId} status updated to {newStatus}."
            };

            int targetUserId = newStatus switch
            {
                "Accepted" or "Rejected" => job.CustomerID,
                "Completed" => job.CustomerID,
                "Cancelled" => job.LabourerID,
                _ => job.CustomerID
            };

            await _notificationService.CreateAsync(targetUserId, message, "StatusUpdate");
            return true;
        }

        public async Task<JobRequest?> GetByIdAsync(int requestId)
        {
            return await _jobRepo.GetById(requestId);
        }

        public async Task<IEnumerable<JobRequest>> GetAllAsync()
        {
            return await _jobRepo.GetAll();
        }

        public async Task<IEnumerable<JobRequest>> GetForCustomerAsync(int customerId)
        {
            return await _jobRepo.GetForCustomer(customerId);
        }

        public async Task<IEnumerable<JobRequest>> GetForLabourerAsync(int labourerId)
        {
            return await _jobRepo.GetForLabourer(labourerId);
        }

        public async Task<bool> CheckAvailabilityAsync(int labourerId, DateTime date, TimeSpan time)
        {
            return await _jobRepo.CheckAvailability(labourerId, date, time);
        }
    }
}
