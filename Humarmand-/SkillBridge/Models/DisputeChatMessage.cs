using System;

namespace SkillBridge.Models
{
    public class DisputeChatMessage
    {
        public int MessageID { get; set; }
        public int SessionID { get; set; }
        public int SenderUserID { get; set; }
        public string SenderName { get; set; } = "";
        public string SenderRole { get; set; } = "";
        public string MessageText { get; set; } = "";
        public DateTime SentAt { get; set; }
    }
}
