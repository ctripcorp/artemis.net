using System.Collections.Generic;
namespace Com.Ctrip.Soa.Artemis.Common.Discovery
{
    public class DiscoveryConfig
    {
        public string ServiceId { get; set; }
        public string RegionId { get; set; }
        public string ZoneId { get; set; }
        public Dictionary<string, string> DiscoveryData { get; set; }

        public override string ToString()
        {
            return "DiscoveryConfig{" +
               "serviceId='" + ServiceId + '\'' +
               ", regionId='" + RegionId + '\'' +
               ", zoneId='" + ZoneId + '\'' +
               ", discoveryData=" + DiscoveryData +
               '}';
        }

    }
}
