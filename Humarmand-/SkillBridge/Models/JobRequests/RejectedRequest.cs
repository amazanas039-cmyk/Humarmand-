using System;

namespace SkillBridge.Models.JobRequests
{
    public class RejectedRequest : JobRequest
    {
        public RejectedRequest()
        {
            Status = "Rejected";
        }

        public override void TransitionTo(string newStatus)
        {
            throw new InvalidOperationException("Cannot transition from a rejected job request.");
        }
    }
}
