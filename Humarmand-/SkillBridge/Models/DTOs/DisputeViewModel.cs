using System;
using System.Collections.Generic;

namespace SkillBridge.Models.DTOs
{
    public class DisputeViewModel
    {
        public int DisputeID { get; set; }
        public int RequestID { get; set; }
        public string CustomerName { get; set; } = "";
        public int CustomerUserID { get; set; }
        public string LabourerName { get; set; } = "";
        public int LabourerUserID { get; set; }
        public string Reason { get; set; } = "";
        public string Status { get; set; } = "";
        public string? ResolutionText { get; set; }
        public DateTime RaisedAt { get; set; }
        public List<ChatMessageViewModel> Messages { get; set; } = new();
        public bool HasActiveSession { get; set; }
    }

    public class ChatMessageViewModel
    {
        public string SenderName { get; set; } = "";
        public string SenderRole { get; set; } = "";
        public string Text { get; set; } = "";
        public DateTime SentAt { get; set; }
    }
}
