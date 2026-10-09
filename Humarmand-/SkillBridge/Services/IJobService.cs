using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SkillBridge.Models;

namespace SkillBridge.Services
{
    public interface IJobService
    {
        Task<int> SendHireRequestAsync(JobRequest request);
        Task<bool> UpdateStatusAsync(int requestId, string newStatus, string? reason);
        Task<JobRequest?> GetByIdAsync(int requestId);
        Task<IEnumerable<JobRequest>> GetAllAsync();
        Task<IEnumerable<JobRequest>> GetForCustomerAsync(int customerId);
        Task<IEnumerable<JobRequest>> GetForLabourerAsync(int labourerId);
        Task<bool> CheckAvailabilityAsync(int labourerId, DateTime date, TimeSpan time);
    }
}
