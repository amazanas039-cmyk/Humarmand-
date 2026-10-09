namespace SkillBridge.Models
{
    public class Customer : User
    {
        public int CustomerID { get; set; }
        public string? Address { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        public override string GetDashboardUrl() => "/Customer/Dashboard";
    }
}
