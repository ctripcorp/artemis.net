using System.Collections.Generic;

namespace Com.Ctrip.Soa.Artemis.Common.Registry
{
    public class HeartbeatRequest : IHasInstance
    {
        public List<Instance> Instances { get; set; }
    }
}
