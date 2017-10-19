using System;
using Com.Ctrip.Soa.Artemis.Client.Common;
using Com.Ctrip.Soa.Artemis.Client.Discovery;
using System.Collections.Generic;
using Com.Ctrip.Soa.Artemis.Common.Configuration;
using Com.Ctrip.Soa.Artemis.Common;
using Com.Ctrip.Soa.Artemis.Common.Discovery;
using System.Linq;
using Com.Ctrip.Soa.Caravan.Configuration;

namespace Com.Ctrip.Soa.Artemis.Client.Test.Utils
{
    public class Constants
    {
        private static IConfigurationManager configurationManager;
        public static IConfigurationManager ConfigurationManager
        {
            get
            {
                if (configurationManager != null)
                    return configurationManager;
                var memoryConfiguration = new MemoryConfiguration();
                memoryConfiguration[ArtemisServiceDomainKey] = ArtemisServiceDomain;
                var dynamicConfigurationSource = ObjectFactory.CreateDefaultDynamicConfigurationSource(0, "apollo-FX.soa.artemis.client", memoryConfiguration);
                return configurationManager = ObjectFactory.CreateDefaultConfigurationManager(dynamicConfigurationSource);
            }
        }
        
        public const string ClientId = "client";
        public static readonly string ArtemisServiceDomain = "http://artemis.soa.fx.fws.qa.nt.ctripcorp.com/artemis-service";
        //public static readonly string ArtemisServiceDomain = "http://10.32.21.10:8080/artemis-web/";
        public static readonly string ArtemisServiceDomainKey = ClientId + ".service.domain.url";
        public static readonly ArtemisClientManagerConfig ManagerConfig = new ArtemisClientManagerConfig(ConfigurationManager);
        public static readonly ArtemisClientConfig DiscoveryClientConfig = new ArtemisClientConfig(ClientId, ManagerConfig,
            AddressManager.NewDiscoveryAddressManager(ClientId, ManagerConfig));
        public static readonly ArtemisClientConfig RegistryClientConfig = new ArtemisClientConfig(ClientId, ManagerConfig,
            AddressManager.NewRegistryAddressManager(ClientId, ManagerConfig));
        public static readonly string RegistryServiceKey = "framework.soa.v1.registryservice";
        public static readonly string JavaRegistryServiceKey = "framework.soa4j.registryservice.v1.registryservice";

        static Constants()
        {
            DeploymentConfig.Init("SHA", "SHAJQ");
        }

        public static Instance NewInstance()
        {
            return new Instance()
            {
                ServiceId = Guid.NewGuid().ToString(),
                InstanceId = Guid.NewGuid().ToString(),
                IP = Guid.NewGuid().ToString(),
                Url = Guid.NewGuid().ToString(),
                RegionId = DeploymentConfig.RegionId,
                ZoneId = DeploymentConfig.ZoneId,
                Status = Guid.NewGuid().ToString()
            };
        }

        public static Instance NewInstance(string serviceId)
        {
            Instance instance = NewInstance();
            instance.ServiceId = serviceId;
            return instance;
        }

        public static string NewServiceId()
        {
            return Guid.NewGuid().ToString();
        }

        public static DiscoveryConfig NewDiscoveryConfig()
        {
            return new DiscoveryConfig()
            {
                ServiceId = NewServiceId()
            };
        }

        public static InstanceChange newInstanceChange(string changeType, Instance instance)
        {
            return new InstanceChange()
            {
                ChangeType = changeType, 
                Instance = instance
            };
        }
    }
}
