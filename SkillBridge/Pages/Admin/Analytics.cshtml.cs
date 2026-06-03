using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SkillBridge.Models;
using SkillBridge.Services;

namespace SkillBridge.Pages.Admin
{
    public record CategoryBreakdownVm(string Name, int Count, int Percent);

    public class AnalyticsModel : PageModel
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly ILabourerService _labourerService;

        public AnalyticsModel(IAnalyticsService analyticsService, ILabourerService labourerService)
        {
            _analyticsService = analyticsService;
            _labourerService = labourerService;
        }

        public int TotalJobsCompleted { get; set; }
        public double AverageRating { get; set; }
        public int TotalDisputes { get; set; }
        public int CompletionRate { get; set; }
        public List<SkillBridge.Models.Labourer> TopLabourers { get; set; } = new();
        public List<CategoryBreakdownVm> CategoryBreakdown { get; set; } = new();

        public async Task OnGetAsync()
        {
            try
            {
                var kpis = await _analyticsService.GetPlatformKPIsAsync();
                TotalJobsCompleted = kpis.TotalJobsCompleted;
                AverageRating = kpis.AvgRating;
                TotalDisputes = kpis.TotalDisputes;
                CompletionRate = (int)kpis.CompletionRate;

                // Top N rated labourers
                var topLabourers = await _analyticsService.GetTopRatedAsync(5);
                TopLabourers = topLabourers.ToList();

                // Category breakdown
                var catDistribution = await _analyticsService.GetCategoryDistributionAsync();
                var catList = catDistribution.ToList();
                int totalCategoryJobs = catList.Sum(c => c.Count);
                CategoryBreakdown = catList.Select(c => new CategoryBreakdownVm(
                    c.Name,
                    c.Count,
                    totalCategoryJobs > 0 ? (int)((double)c.Count / totalCategoryJobs * 100) : 0
                )).ToList();
            }
            catch (Exception)
            {
                TopLabourers = new List<SkillBridge.Models.Labourer>();
                CategoryBreakdown = new List<CategoryBreakdownVm>();
            }
        }
    }
}
