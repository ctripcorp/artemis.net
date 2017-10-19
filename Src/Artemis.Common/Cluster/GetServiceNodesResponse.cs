using System.Collections.Generic;

namespace Com.Ctrip.Soa.Artemis.Common.Cluster
{
    public class GetServiceNodesResponse : IHasResponseStatus
    {
        public ResponseStatus ResponseStatus { get; set; }
        public List<ServiceNode> Nodes { get; set; }
    }
}
