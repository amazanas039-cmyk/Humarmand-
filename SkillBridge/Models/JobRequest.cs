using System;
using System.Collections.Generic;

namespace SkillBridge.Models
{
    public abstract class JobRequest
    {
        public int RequestID { get; set; }
        public int CustomerID { get; set; }
        public string CustomerName { get; set; } = "";
        public int CustomerUserId { get; set; }
        public int LabourerID { get; set; }
        public string LabourerName { get; set; } = "";
        public string CategoryName { get; set; } = "";
        public int LabourerUserId { get; set; }
        public string JobDescription { get; set; } = "";
        public DateTime PreferredDate { get; set; }
        public TimeSpan PreferredTime { get; set; }
        public string Location { get; set; } = "";
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string Status { get; set; } = "";
        public string? RejectionReason { get; set; }
        public decimal EstimatedHours { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        private readonly List<INotificationObserver> _observers = new();
        
        public void AddObserver(INotificationObserver o) => _observers.Add(o);
        
        public void RemoveObserver(INotificationObserver o) => _observers.Remove(o);

        public abstract void TransitionTo(string newStatus);
        
        protected void NotifyObservers(string message)
        {
            foreach (var observer in _observers)
            {
                observer.Notify(this, message);
            }
        }
    }
}
