using System;

namespace SkillBridge.Models.JobRequests
{
    public class PendingRequest : JobRequest
    {
        public PendingRequest()
        {
            Status = "Pending";
        }

        public override void TransitionTo(string newStatus)
        {
            if (newStatus == "Accepted" || newStatus == "Rejected")
            {
                Status = newStatus;
                NotifyObservers($"Your job request has been {newStatus.ToLower()} by the laborer.");
            }
            else
            {
                throw new InvalidOperationException($"Cannot transition from Pending to {newStatus}");
            }
        }
    }
}
