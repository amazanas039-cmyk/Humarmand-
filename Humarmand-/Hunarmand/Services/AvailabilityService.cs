using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Hunarmand.Repositories;

namespace Hunarmand.Services
{
    public class AvailabilityService : IAvailabilityService
    {
        private readonly JobRepository _jobRepo;
        private readonly ILogger<AvailabilityService> _logger;

        public AvailabilityService(JobRepository jobRepo, ILogger<AvailabilityService> logger)
        {
            _jobRepo = jobRepo;
            _logger = logger;
        }

        public async Task<bool> CheckAvailabilityAsync(int labourerId, DateTime date, TimeSpan time)
        {
            bool available = await _jobRepo.CheckAvailability(labourerId, date, time);
            _logger.LogDebug("Availability check for labourer {LID} on {Date} at {Time}: {Result}",
                labourerId, date, time, available);
            return available;
        }
    }
}
