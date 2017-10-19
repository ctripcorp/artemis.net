using Com.Ctrip.Soa.Artemis.Common.Configuration;
using Com.Ctrip.Soa.Caravan.Metric;
using Com.Ctrip.Soa.Caravan.Metric.Null;
using Com.Ctrip.Soa.Artemis.Common.Condition;
using Com.Ctrip.Soa.Caravan.Utility;
using Com.Ctrip.Soa.Caravan.Configuration;

namespace Com.Ctrip.Soa.Artemis.Client
{
    public class ArtemisClientManagerConfig
    {
        public ArtemisClientManagerConfig(IConfigurationManager configurationManager)
            : this(configurationManager, NullEventMetricManager.Instance, NullAuditMetricManager.Instance)
        { }

        public ArtemisClientManagerConfig(IConfigurationManager configurationManager, 
            IEventMetricManager eventMetricManager, IAuditMetricManager auditMetricManager)
            : this(configurationManager, eventMetricManager, auditMetricManager, new RegistryClientConfig(), new DiscoveryClientConfig())
        { }

        public ArtemisClientManagerConfig(IConfigurationManager configurationManager, 
            IEventMetricManager eventMetricManager, IAuditMetricManager auditMetricManager, RegistryClientConfig registryClientConfig)
            : this(configurationManager, eventMetricManager, auditMetricManager, registryClientConfig, new DiscoveryClientConfig())
        { }

        public ArtemisClientManagerConfig(IConfigurationManager configurationManager,
            IEventMetricManager eventMetricManager, IAuditMetricManager auditMetricManager, DiscoveryClientConfig discoveryClientConfig)
            : this(configurationManager, eventMetricManager, auditMetricManager, new RegistryClientConfig(), discoveryClientConfig)
        { }

        public ArtemisClientManagerConfig(IConfigurationManager configurationManager,
            IEventMetricManager eventMetricManager, IAuditMetricManager auditMetricManager,
            RegistryClientConfig registryClientConfig, DiscoveryClientConfig discoveryClientConfig)
        {
            Preconditions.CheckArgument(configurationManager != null, "configurationManager");
            Preconditions.CheckArgument(eventMetricManager != null, "event metric manager");
            Preconditions.CheckArgument(auditMetricManager != null, "audit metric manager");
            Preconditions.CheckArgument(registryClientConfig != null, "registry client config");
            Preconditions.CheckArgument(discoveryClientConfig != null, "discovery client config");

            ConfigurationManager = configurationManager;
            EventMetricManager = eventMetricManager;
            AuditMetricManager = auditMetricManager;
            RegistryClientConfig = registryClientConfig;
            DiscoveryClientConfig = discoveryClientConfig;
        }

        public IConfigurationManager ConfigurationManager { get; private set; }

        public IEventMetricManager EventMetricManager { get; private set; }

        public IAuditMetricManager AuditMetricManager { get; private set; }

        public DiscoveryClientConfig DiscoveryClientConfig { get; private set; }

        public RegistryClientConfig RegistryClientConfig { get; private set; }
    }
}
