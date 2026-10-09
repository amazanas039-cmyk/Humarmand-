using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Hunarmand.Models;
using Hunarmand.Repositories;
using Hunarmand.Services;

namespace Hunarmand.Pages.Admin
{
    // ---- lightweight view-model records ----
    public record RecentJobVm(string CustomerName, char CustomerInitial, string Category, DateTime Date, string Status);
    public record ActivityVm(string Actor, string Action, string DotClass, string Icon, string TimeAgo);
    public record CategoryStatVm(string Name, int Count, int Percent);
    public record PendingVerificationVm(int Id, string Name, char Initial, string Skill);

    public class DashboardModel : PageModel
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly IJobService _jobService;
        private readonly ILabourerService _labourerService;
        private readonly NotificationRepository _notificationRepo;

        public DashboardModel(
            IAnalyticsService analyticsService,
            IJobService jobService,
            ILabourerService labourerService,
            NotificationRepository notificationRepo)
        {
            _analyticsService = analyticsService;
            _jobService = jobService;
            _labourerService = labourerService;
            _notificationRepo = notificationRepo;
        }

        // ---------- KPI stats ----------
        public int    TotalUsers              { get; private set; }
        public int    ActiveJobs              { get; private set; }
        public int    PendingDisputes         { get; private set; }
        public int    PendingVerifications    { get; private set; }
        public int    TotalLabourers          { get; private set; }
        public double AverageRating           { get; private set; }

        // ---------- Lists ----------
        public List<RecentJobVm>           RecentJobs              { get; private set; } = new();
        public List<ActivityVm>            RecentActivity          { get; private set; } = new();
        public List<CategoryStatVm>        CategoryStats           { get; private set; } = new();
        public List<PendingVerificationVm> PendingVerificationList { get; private set; } = new();

        public async Task OnGetAsync()
        {
            try
            {
                // ---- KPIs ----
                var kpis = await _analyticsService.GetPlatformKPIsAsync();
                TotalUsers           = kpis.TotalUsers;
                ActiveJobs           = kpis.ActiveJobs;
                PendingDisputes      = kpis.OpenDisputes;
                PendingVerifications = kpis.PendingVerifications;
                TotalLabourers       = kpis.ActiveLabourers;
                AverageRating        = kpis.AvgRating;

                // ---- Recent jobs ----
                var recentJobRequests = await _jobService.GetAllAsync();
                RecentJobs = recentJobRequests.Take(5).Select(j => new RecentJobVm(
                    j.CustomerName,
                    string.IsNullOrEmpty(j.CustomerName) ? '?' : j.CustomerName[0],
                    j.CategoryName,
                    j.CreatedAt,
                    j.Status
                )).ToList();

                // ---- Activity feed (From system notifications) ----
                var recentNotifs = await _notificationRepo.GetRecentNotifications(5);
                RecentActivity = recentNotifs.Select(x => {
                    var (n, actor) = x;
                    string dotClass = n.NotificationType switch {
                        "Verification" or "VerificationApproved" => "success",
                        "VerificationRejected" or "Dispute" or "DisputeStrikes" => "danger",
                        "StatusUpdate" or "NewRequest" or "StatusChange" => "info",
                        _ => "info"
                    };
                    string icon = n.NotificationType switch {
                        "Verification" or "VerificationApproved" => "check",
                        "VerificationRejected" => "triangle-exclamation",
                        "Dispute" or "DisputeStrikes" => "flag",
                        "NewRequest" => "briefcase",
                        _ => "bell"
                    };
                    
                    return new ActivityVm(actor, n.Message, dotClass, icon, GetTimeAgo(n.CreatedAt));
                }).ToList();

                if (!RecentActivity.Any())
                {
                    RecentActivity.Add(new ActivityVm("System", "Dashboard is operational. Awaiting live activity log...", "info", "info", "Just now"));
                }

                // ---- Category bar chart ----
                var catDistribution = await _analyticsService.GetCategoryDistributionAsync();
                var catList = catDistribution.ToList();
                int totalCategoryJobs = catList.Sum(c => c.Count);
                CategoryStats = catList.Select(c => new CategoryStatVm(
                    c.Name,
                    c.Count,
                    totalCategoryJobs > 0 ? (int)((double)c.Count / totalCategoryJobs * 100) : 0
                )).ToList();

                // ---- Pending verifications ----
                var pendingLabourers = await _labourerService.GetPendingVerificationAsync();
                PendingVerificationList = pendingLabourers.Select(l => new PendingVerificationVm(
                    l.LabourerID,
                    l.FullName,
                    string.IsNullOrEmpty(l.FullName) ? '?' : l.FullName[0],
                    l.CategoryName
                )).ToList();
            }
            catch (Exception ex)
            {
                System.IO.File.WriteAllText("dashboard_error.txt", ex.ToString());
                // Fallback in case database is empty or connection fails temporarily
                TotalUsers = 0;
                ActiveJobs = 0;
                PendingDisputes = 0;
                PendingVerifications = 0;
                TotalLabourers = 0;
                AverageRating = 0.0;
            }
        }

        private string GetTimeAgo(DateTime dt)
        {
            var span = DateTime.UtcNow - dt;
            if (span.TotalMinutes < 1) return "Just now";
            if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes} min ago";
            if (span.TotalHours < 24) return $"{(int)span.TotalHours} hr ago";
            return $"{(int)span.TotalDays} days ago";
        }
    }
}
