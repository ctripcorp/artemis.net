using Com.Ctrip.Soa.Artemis.Client.Common;
using System;
using System.Collections.Generic;
using Com.Ctrip.Soa.Artemis.Common;
using Com.Ctrip.Soa.Artemis.Common.Discovery;
using Com.Ctrip.Soa.Artemis.Common.Configuration;
using Com.Ctrip.Soa.Artemis.Common.Condition;
using System.Linq;
using Com.Ctrip.Soa.Caravan.Logging;

namespace Com.Ctrip.Soa.Artemis.Client.Discovery
{
    /// <summary>
    /// Created by fang_j 2016/08/03.
    /// </summary>
    public class ArtemisDiscoveryHttpClient : ArtemisHttpClient
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(ArtemisDiscoveryHttpClient));

        public ArtemisDiscoveryHttpClient(ArtemisClientConfig config)
            : base(config, config.Key("discovery"))
        {
        }

        public Service GetService(DiscoveryConfig discoveryConfig)
        {
            Preconditions.CheckArgument(discoveryConfig != null, "discoveryConfig");
            List<Service> services = GetServices(new List<DiscoveryConfig>() { discoveryConfig });
            if (services.Count > 0)
            {
                return services[0];
            }

            throw new Exception("not found any service by discoveyConfig:" + discoveryConfig);
        }

        public List<Service> GetServices(List<DiscoveryConfig> discoveryConfigs)
        {
            Preconditions.CheckArgument(!Conditions.IsNullOrEmpty(discoveryConfigs), "discoveryConfigs should not be null or empty");

            LookupRequest request = new LookupRequest()
            {
                DiscoveryConfigs = discoveryConfigs,
                RegionId = DeploymentConfig.RegionId,
                ZoneId = DeploymentConfig.ZoneId
            };
            LookupResponse response = this.Request<LookupResponse>(RestPaths.DISCOVERY_LOOKUP_FULL_PATH, request);
            LogEvent(response.ResponseStatus, "discovery", "lookup");
            if (response.ResponseStatus.IsSuccess())
                return response.Services;

            throw new Exception("lookup services failed. " + response.ResponseStatus);
        }
    }
}
