using System;

namespace Hunarmand.Models.JobRequests
{
    public class CompletedRequest : JobRequest
    {
        public CompletedRequest()
        {
            Status = "Completed";
        }

        public override void TransitionTo(string newStatus)
        {
            throw new InvalidOperationException("Cannot transition from a completed job request.");
        }
    }
}
