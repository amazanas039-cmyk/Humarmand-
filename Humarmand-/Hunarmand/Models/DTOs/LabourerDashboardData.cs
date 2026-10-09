using System.Collections.Generic;

namespace Hunarmand.Models.DTOs
{
    public class LabourerDashboardData
    {
        public decimal ReputationScore { get; set; }
        public double ReputationDelta { get; set; }
        public decimal EstimatedEarningsTotal { get; set; }
        public decimal EarningsThisMonth { get; set; }
        public double CompletionRate { get; set; }
        public int ActiveRequests { get; set; }
        public List<DataPoint> EarningsTrend { get; set; } = new();
        public List<DataPoint> RatingTrend { get; set; } = new();
        public JobFunnel Funnel { get; set; } = new();
        public List<JobRequest> RecentRequests { get; set; } = new();
    }

    public class JobFunnel
    {
        public int TotalRequests { get; set; }
        public int Accepted { get; set; }
        public int Completed { get; set; }
        public int Rejected { get; set; }
    }
}
