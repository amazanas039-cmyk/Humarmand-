using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Hunarmand.Helpers;
using Hunarmand.Models;
using Hunarmand.Models.DTOs;
using Hunarmand.Services;

namespace Hunarmand.Pages.Labourer
{
    [RequireRole("Labourer")]
    public class DashboardModel : PageModel
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly IReviewService _reviewService;

        public DashboardModel(IAnalyticsService analyticsService, IReviewService reviewService)
        {
            _analyticsService = analyticsService;
            _reviewService = reviewService;
        }

        public string ProfessionalName { get; set; } = "";
        public LabourerDashboardData DashboardData { get; set; } = new();
        public IEnumerable<Review> RecentReviews { get; set; } = new List<Review>();

        public async Task<IActionResult> OnGetAsync()
        {
            ProfessionalName = HttpContext.Session.GetString(SessionKeys.UserFullName) ?? "Professional";
            var labourerIdStr = HttpContext.Session.GetString(SessionKeys.LabourerId);
            
            if (string.IsNullOrEmpty(labourerIdStr) || !int.TryParse(labourerIdStr, out int labourerId))
            {
                return RedirectToPage("/Auth/LabourerLogin");
            }

            DashboardData = await _analyticsService.GetLabourerDashboardAsync(labourerId, 30);
            RecentReviews = await _reviewService.GetForLabourerAsync(labourerId);

            return Page();
        }
    }
}
