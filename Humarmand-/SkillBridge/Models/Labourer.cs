namespace SkillBridge.Models
{
    public class Labourer : User
    {
        public int LabourerID { get; set; }
        public int CategoryID { get; set; }
        public string CategoryName { get; set; } = "";
        public int ExperienceYears { get; set; }
        public decimal HourlyRate { get; set; }
        public string? Bio { get; set; }
        public string? AvailabilitySchedule { get; set; } // JSON
        public string? Address { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public decimal ReputationScore { get; set; }
        public decimal CompletionRate { get; set; }
        public string VerificationStatus { get; set; } = "Pending";
        public string? VerificationNote { get; set; }
        public bool IsProfileLive { get; set; }
        
        // Computed/Strategy variables
        public double? DistanceKm { get; set; }
        public double SmartScore { get; set; }

        public void RecalculateReputation(decimal newScore) => ReputationScore = newScore;
        public override string GetDashboardUrl() => "/Labourer/Dashboard";
    }
}
