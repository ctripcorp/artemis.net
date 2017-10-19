using System;
using Com.Ctrip.Soa.Artemis.Client.Common;
using Com.Ctrip.Soa.Artemis.Common.Configuration;
using Com.Ctrip.Soa.Artemis.Common.Condition;
using Com.Ctrip.Soa.Artemis.Common;
using Com.Ctrip.Soa.Artemis.Common.Discovery;
using Com.Ctrip.Soa.Caravan.Logging;

namespace Com.Ctrip.Soa.Artemis.Client.Discovery
{
    /// <summary>
    /// Created by fang_j 2016/08/01.
    /// </summary>
    public class DiscoveryClientImpl : DiscoveryClient
    {
        private readonly ServiceRepository _serviceRepository;
        public DiscoveryClientImpl(string clientId, ArtemisClientManagerConfig managerConfig)
        {
            Preconditions.CheckArgument(!string.IsNullOrWhiteSpace(clientId), "clientId");
            Preconditions.CheckArgument(managerConfig != null, "manager config");

            ArtemisClientConfig clientConfig = new ArtemisClientConfig(clientId, managerConfig,
                AddressManager.NewDiscoveryAddressManager(clientId, managerConfig));
            _serviceRepository = new ServiceRepository(clientConfig);
        }

        public Service GetService(DiscoveryConfig discoveryConfig)
        {
            return _serviceRepository.GetService(discoveryConfig);
        }

        public void RegisterServiceChangeListener(DiscoveryConfig discoveryConfig, ServiceChangeListener listener)
        {
            this._serviceRepository.RegisterServiceChangeListener(discoveryConfig, listener);
        }
    }
}
