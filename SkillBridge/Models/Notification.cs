using System;

namespace SkillBridge.Models
{
    public class Notification
    {
        public int NotificationID { get; set; }
        public int UserID { get; set; }
        public string Message { get; set; } = "";
        public string? NotificationType { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
