using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SkillBridge.Helpers;
using SkillBridge.Models;
using SkillBridge.Services;

namespace SkillBridge.Pages.Customer
{
    [RequireRole("Customer")]
    public class DashboardModel : PageModel
    {
        private readonly IJobService _jobService;
        private readonly ICategoryService _categoryService;
        private readonly INotificationService _notificationService;

        public DashboardModel(IJobService jobService, ICategoryService categoryService, INotificationService notificationService)
        {
            _jobService = jobService;
            _categoryService = categoryService;
            _notificationService = notificationService;
        }

        public string CustomerName { get; set; } = "";
        public int ActiveJobsCount { get; set; }
        public int CompletedJobsCount { get; set; }
        public int PendingReviewCount { get; set; }
        
        public IEnumerable<JobRequest> RecentJobs { get; set; } = new List<JobRequest>();
        public IEnumerable<SkillCategory> Categories { get; set; } = new List<SkillCategory>();

        public async Task<IActionResult> OnGetAsync()
        {
            CustomerName = HttpContext.Session.GetString(SessionKeys.UserFullName) ?? "Customer";
            var customerIdStr = HttpContext.Session.GetString(SessionKeys.CustomerId);
            
            if (string.IsNullOrEmpty(customerIdStr) || !int.TryParse(customerIdStr, out int customerId))
            {
                return RedirectToPage("/Auth/CustomerLogin");
            }

            var jobs = await _jobService.GetForCustomerAsync(customerId);
            
            ActiveJobsCount = jobs.Count(j => j.Status == "Pending" || j.Status == "Accepted" || j.Status == "InProgress");
            CompletedJobsCount = jobs.Count(j => j.Status == "Completed");
            
            // Assuming we check pending reviews via a hypothetical logic (or simply if they are Completed)
            // For now, let's just consider Completed jobs
            PendingReviewCount = jobs.Count(j => j.Status == "Completed"); // To be refined

            RecentJobs = jobs.OrderByDescending(j => j.CreatedAt).Take(5);
            Categories = _categoryService.GetAllCategories();

            return Page();
        }
    }
}
