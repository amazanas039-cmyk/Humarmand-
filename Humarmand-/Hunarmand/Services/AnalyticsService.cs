using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Hunarmand.Models;
using Hunarmand.Models.DTOs;
using Hunarmand.Repositories;

namespace Hunarmand.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly AnalyticsRepository _analyticsRepo;
        private readonly JobRepository _jobRepo;
        private readonly ILogger<AnalyticsService> _logger;

        public AnalyticsService(AnalyticsRepository analyticsRepo, JobRepository jobRepo, ILogger<AnalyticsService> logger)
        {
            _analyticsRepo = analyticsRepo;
            _jobRepo = jobRepo;
            _logger = logger;
        }

        public async Task<PlatformKPIs> GetPlatformKPIsAsync()
        {
            return await _analyticsRepo.GetPlatformKPIs();
        }

        public async Task<IEnumerable<DataPoint>> GetUserGrowthAsync(int days)
        {
            return await _analyticsRepo.GetUserGrowth(days);
        }

        public async Task<IEnumerable<DataPoint>> GetDailyDealsAsync(int days)
        {
            return await _analyticsRepo.GetDailyDeals(days);
        }

        public async Task<IEnumerable<CategoryStat>> GetCategoryDistributionAsync()
        {
            return await _analyticsRepo.GetCategoryDistribution();
        }

        public async Task<IEnumerable<Labourer>> GetTopRatedAsync(int n)
        {
            return await _analyticsRepo.GetTopRatedLabourers(n);
        }

        public async Task<IEnumerable<CategoryStat>> GetUnmetDemandAsync()
        {
            return await _analyticsRepo.GetUnmetDemand();
        }

        public async Task<LabourerDashboardData> GetLabourerDashboardAsync(int labourerId, int days)
        {
            var earningsTask = _analyticsRepo.GetLabourerEarnings(labourerId, days);
            var ratingTask = _analyticsRepo.GetLabourerRatingTrend(labourerId, days);
            var funnelTask = _analyticsRepo.GetLabourerJobFunnel(labourerId);
            var recentTask = _jobRepo.GetForLabourer(labourerId);

            await Task.WhenAll(earningsTask, ratingTask, funnelTask, recentTask);

            var earnings = (await earningsTask).ToList();
            var funnel = await funnelTask;
            var recentJobs = (await recentTask).ToList();

            return new LabourerDashboardData
            {
                EarningsTrend = earnings,
                EstimatedEarningsTotal = (decimal)earnings.Sum(e => e.Value),
                EarningsThisMonth = (decimal)earnings.TakeLast(30).Sum(e => e.Value),
                RatingTrend = (await ratingTask).ToList(),
                Funnel = funnel,
                ActiveRequests = recentJobs.Count(j => j.Status == "Accepted"),
                CompletionRate = funnel.TotalRequests > 0
                    ? Math.Round((double)funnel.Completed / funnel.TotalRequests * 100, 1)
                    : 0,
                RecentRequests = recentJobs.Take(10).ToList()
            };
        }
    }
}
