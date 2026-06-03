using System;
using System.Threading.Tasks;

namespace SkillBridge.Services
{
    public interface IAvailabilityService
    {
        Task<bool> CheckAvailabilityAsync(int labourerId, DateTime date, TimeSpan time);
    }
}
