using System.Collections.Generic;

namespace Com.Ctrip.Soa.Artemis.Common.Discovery
{
    public class LookupResponse : IHasResponseStatus
    {
        public ResponseStatus ResponseStatus {get; set;}

        public List<Service> Services { get; set; }
    }
}
