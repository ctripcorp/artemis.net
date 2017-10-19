using Com.Ctrip.Soa.Artemis.Common.Configuration;
using Com.Ctrip.Soa.Artemis.Common.Condition;
using Com.Ctrip.Soa.Caravan.Metric;
using Com.Ctrip.Soa.Caravan.Configuration;

using Com.Ctrip.Soa.Caravan.Logging;

namespace Com.Ctrip.Soa.Artemis.Client.Common
{
    /// <summary>
    /// Created by fang_j 2016/08/01.
    /// </summary>
    public class ArtemisClientConfig
    {
        private readonly string _clientId;

        public ArtemisClientConfig(string clientId, ArtemisClientManagerConfig config, AddressManager addressManager)
        {
            Preconditions.CheckArgument(!string.IsNullOrWhiteSpace(clientId), "clientId");
            Preconditions.CheckArgument(config != null, "config");
            Preconditions.CheckArgument(addressManager != null, "addressManager");

            this._clientId = clientId;
            ConfigurationManager = config.ConfigurationManager;
            AddressManager = addressManager;
            EventMetricManager = config.EventMetricManager;
            AuditMetricManager = config.AuditMetricManager;
            RegistryClientConfig = config.RegistryClientConfig;
            DiscoveryClientConfig = config.DiscoveryClientConfig;
        }

        public IConfigurationManager ConfigurationManager { get; private set; }
        
        public AddressManager AddressManager{get; private set;}

        public IEventMetricManager EventMetricManager { get; private set; }

        public IAuditMetricManager AuditMetricManager { get; private set; }

        public string Key(string suffix)
        {
            Preconditions.CheckArgument(!string.IsNullOrWhiteSpace(suffix), "suffix");
            return _clientId + "." + suffix;
        }

        public RegistryClientConfig RegistryClientConfig { get; private set; }

        public DiscoveryClientConfig DiscoveryClientConfig { get; private set; }
    }
}
