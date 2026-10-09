using System;

namespace Hunarmand.Models
{
    public class Dispute
    {
        public int DisputeID { get; set; }
        public int RequestID { get; set; }
        public int RaisedByUserID { get; set; }
        public string RaisedByName { get; set; } = "";
        public string RaisedByRole { get; set; } = "";
        public string Reason { get; set; } = "";
        public string Status { get; set; } = "Open"; // Open, Resolved
        public string? Resolution { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        
        // Extended attributes
        public int CustomerUserID { get; set; }
        public string CustomerName { get; set; } = "";
        public int LabourerUserID { get; set; }
        public string LabourerName { get; set; } = "";
        public string JobDescription { get; set; } = "";
    }
}
