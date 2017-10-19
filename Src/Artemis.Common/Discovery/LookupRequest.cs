using System.Collections.Generic;

namespace Com.Ctrip.Soa.Artemis.Common.Discovery
{
    public class LookupRequest
    {
        public string RegionId { get; set; }

        public string ZoneId { get; set; }

        public List<DiscoveryConfig> DiscoveryConfigs { get; set; }
    }
}