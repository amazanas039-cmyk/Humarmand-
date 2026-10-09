using System;

namespace Hunarmand.Models.JobRequests
{
    public class AcceptedRequest : JobRequest
    {
        public AcceptedRequest()
        {
            Status = "Accepted";
        }

        public override void TransitionTo(string newStatus)
        {
            if (newStatus == "InProgress")
            {
                Status = newStatus;
                NotifyObservers("Your job request is now in progress.");
            }
            else
            {
                throw new InvalidOperationException($"Cannot transition from Accepted to {newStatus}");
            }
        }
    }
}
