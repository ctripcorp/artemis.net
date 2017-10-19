using System.Collections.Concurrent;
using Com.Ctrip.Soa.Artemis.Client.Registry;
using Com.Ctrip.Soa.Artemis.Client.Discovery;
using Com.Ctrip.Soa.Artemis.Common.Configuration;
using Com.Ctrip.Soa.Artemis.Common.Condition;

namespace Com.Ctrip.Soa.Artemis.Client
{
    /// <summary>
    /// Created by fang_j 2016/08/01.
    /// </summary>
    public class ArtemisClientManager
    {
        private static readonly ConcurrentDictionary<string, ArtemisClientManager> _managers = new ConcurrentDictionary<string, ArtemisClientManager>();
        private readonly string _managerId;
        private readonly string _clientId;
        private readonly ArtemisClientManagerConfig _config;
        private volatile RegistryClient _registryClient;
        private volatile DiscoveryClient _discoveryClient;

        private ArtemisClientManager(string managerId, ArtemisClientManagerConfig config)
        {
            Preconditions.CheckArgument(!string.IsNullOrWhiteSpace(managerId), "managerId");
            Preconditions.CheckArgument(config != null, "config");

            this._managerId = managerId;
            this._clientId = "artemis.client." + managerId;
            this._config = config;
        }

        public DiscoveryClient DiscoveryClient
        {
            get {
                if (this._discoveryClient == null)
                {
                    lock (this)
                    {
                        if (this._discoveryClient == null) {
                            this._discoveryClient = new DiscoveryClientImpl(this._clientId, this._config);
                        }
                    }
                }
                return this._discoveryClient; 
            }
        }

        public RegistryClient RegistryClient
        {
            get {
                if (this._registryClient == null)
                {
                    lock (this)
                    {
                        if (this._registryClient == null)
                        {
                            this._registryClient = new RegistryClientImpl(this._clientId, this._config);
                        }
                    }
                }
                return this._registryClient; 
            }
        }

        public static ArtemisClientManager getManager(string managerId, ArtemisClientManagerConfig managerConfig)
        {
            Preconditions.CheckArgument(!string.IsNullOrWhiteSpace(managerId), "managerId");
            Preconditions.CheckArgument(managerConfig != null, "manager config");

            return _managers.GetOrAdd(managerId, key =>
            {
                return new ArtemisClientManager(managerId, managerConfig);
            });
        }
    }
}
