using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Hunarmand.Models;
using Hunarmand.Repositories;

namespace Hunarmand.Pages.Admin
{
    public class JobsModel : PageModel
    {
        private readonly JobRepository _jobRepo;
        public JobsModel(JobRepository jobRepo) { _jobRepo = jobRepo; }

        public List<JobRequest> Jobs { get; set; } = new();
        public List<JobRequest> FilteredJobs { get; set; } = new();
        public string? StatusFilter { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync(string? status)
        {
            StatusFilter = status;
            try
            {
                var allJobs = await _jobRepo.GetAll();
                Jobs = allJobs.ToList();
                FilteredJobs = string.IsNullOrEmpty(status)
                    ? Jobs
                    : Jobs.Where(j => j.Status == status).ToList();
            }
            catch
            {
                ErrorMessage = "Could not load jobs.";
                Jobs = new List<JobRequest>();
                FilteredJobs = new List<JobRequest>();
            }
        }
    }
}
