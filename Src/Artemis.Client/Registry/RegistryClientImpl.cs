using System;
using Com.Ctrip.Soa.Artemis.Client.Common;
using Com.Ctrip.Soa.Artemis.Common.Configuration;
using Com.Ctrip.Soa.Artemis.Common;
using Com.Ctrip.Soa.Artemis.Common.Condition;
using Com.Ctrip.Soa.Caravan.Logging;

namespace Com.Ctrip.Soa.Artemis.Client.Registry
{
    /// <summary>
    /// Created by fang_j 2016/08/01.
    /// </summary>
    public class RegistryClientImpl : RegistryClient
    {
        private readonly InstanceRepository _instanceRepository;
        private readonly InstanceRegistry _instanceRegistry;

        public RegistryClientImpl(string clientId, ArtemisClientManagerConfig managerConfig)
        {
            Preconditions.CheckArgument(!string.IsNullOrWhiteSpace(clientId), "clientId");
            Preconditions.CheckArgument(managerConfig != null, "manager config");
            ArtemisClientConfig clientConfig = new ArtemisClientConfig(clientId, managerConfig,
                AddressManager.NewRegistryAddressManager(clientId, managerConfig));

            this._instanceRepository = new InstanceRepository(clientConfig);
            this._instanceRegistry = new InstanceRegistry(_instanceRepository, clientConfig);
        }

        public void Register(params Instance[] instances)
        {
            this._instanceRepository.Register(instances);
        }

        public void Unregister(params Instance[] instances)
        {
            this._instanceRepository.Unregister(instances);
        }
    }
}
