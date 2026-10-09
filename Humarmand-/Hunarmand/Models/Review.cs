using System;

namespace Hunarmand.Models
{
        public class Review
        {
            public int ReviewID { get; set; }
            public int RequestID { get; set; }
            public int TargetLabourerID { get; set; }
            public int ReviewerUserID { get; set; }
            public string ReviewerName { get; set; } = "";
            public int StarRating { get; set; }
            public string? ReviewText { get; set; }
            public DateTime CreatedAt { get; set; }
            public int Rating => StarRating;
            public string? Comment => ReviewText;
            public string? LabourerName { get; set; }
            public string? ReviewerPhoto { get; set; }
        }
}
