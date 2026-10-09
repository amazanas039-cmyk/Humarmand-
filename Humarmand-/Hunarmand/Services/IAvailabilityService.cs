using System;
using System.Threading.Tasks;

namespace Hunarmand.Services
{
    public interface IAvailabilityService
    {
        Task<bool> CheckAvailabilityAsync(int labourerId, DateTime date, TimeSpan time);
    }
}
