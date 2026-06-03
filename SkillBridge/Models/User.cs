using System;

namespace SkillBridge.Models
{
    public abstract class User
    {
        public int UserID { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public string Role { get; set; } = "";
        public string? Phone { get; set; }
        public bool IsActive { get; set; }
        public bool IsSuspended { get; set; }
        public int DisputeStrikes { get; set; }
        public DateTime CreatedAt { get; set; }

        public abstract string GetDashboardUrl();
    }
}
