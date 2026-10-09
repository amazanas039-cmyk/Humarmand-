using System;

namespace Hunarmand.Models.JobRequests
{
    public class InProgressRequest : JobRequest
    {
        public InProgressRequest()
        {
            Status = "InProgress";
        }

        public override void TransitionTo(string newStatus)
        {
            if (newStatus == "Completed")
            {
                Status = newStatus;
                NotifyObservers("Your job has been completed by the laborer.");
            }
            else
            {
                throw new InvalidOperationException($"Cannot transition from InProgress to {newStatus}");
            }
        }
    }
}
