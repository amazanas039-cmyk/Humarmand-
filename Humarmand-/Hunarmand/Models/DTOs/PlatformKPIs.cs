namespace Hunarmand.Models.DTOs
{
    public class PlatformKPIs
    {
        public int TotalUsers { get; set; }
        public int ActiveLabourers { get; set; }
        public int JobsToday { get; set; }
        public int NewUsersThisWeek { get; set; }
        public double AvgRating { get; set; }
        public double CompletionRate { get; set; }
        public int PendingVerifications { get; set; }
        public int OpenDisputes { get; set; }
        public int ActiveJobs { get; set; }
        public int TotalJobsCompleted { get; set; }
        public int TotalDisputes { get; set; }
        
        // WoW delta metrics
        public double UserGrowthDelta { get; set; }
        public double JobsDelta { get; set; }
    }
}
