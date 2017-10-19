using Com.Ctrip.Soa.Artemis.Common;
using Com.Ctrip.Soa.Artemis.Common.Discovery;

namespace Com.Ctrip.Soa.Artemis.Client
{
    /// <summary>
    /// Created by fang_j 2016/08/01.
    /// </summary>
    public interface DiscoveryClient
    {
        Service GetService(DiscoveryConfig discoveryConfig);
        void RegisterServiceChangeListener(DiscoveryConfig discoveryConfig, ServiceChangeListener listener);
    }
}