namespace SkillBridge.Models
{
    public class Admin : User
    {
        public int AdminID { get; set; }
        public override string GetDashboardUrl() => "/Admin/Dashboard";
    }
}
