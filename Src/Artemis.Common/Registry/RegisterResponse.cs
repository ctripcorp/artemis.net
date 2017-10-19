using System.Collections.Generic;

namespace Com.Ctrip.Soa.Artemis.Common.Registry
{
    public class RegisterResponse : IHasResponseStatus
    {
        public ResponseStatus ResponseStatus { get; set; }
        public List<FailedInstance> FailedInstances { get; set; }
    }
}
