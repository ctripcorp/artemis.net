using System.Collections.Generic;

namespace Com.Ctrip.Soa.Artemis.Common.Registry
{
    public class HeartbeatResponse : IHasResponseStatus
    {
        public ResponseStatus ResponseStatus { get; set; }

        public List<FailedInstance> FailedInstances { get; set; }
    }
}
