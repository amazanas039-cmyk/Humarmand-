using System.Collections.Generic;
using System.Threading.Tasks;
using SkillBridge.Models;
using SkillBridge.Models.DTOs;

namespace SkillBridge.Services
{
    public interface IAnalyticsService
    {
        Task<PlatformKPIs> GetPlatformKPIsAsync();
        Task<IEnumerable<DataPoint>> GetUserGrowthAsync(int days);
        Task<IEnumerable<DataPoint>> GetDailyDealsAsync(int days);
        Task<IEnumerable<CategoryStat>> GetCategoryDistributionAsync();
        Task<IEnumerable<Labourer>> GetTopRatedAsync(int n);
        Task<IEnumerable<CategoryStat>> GetUnmetDemandAsync();
        Task<LabourerDashboardData> GetLabourerDashboardAsync(int labourerId, int days);
    }
}
