using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Com.Ctrip.Soa.Artemis.Client.Test.Utils;
using Com.Ctrip.Soa.Artemis.Common;
using Com.Ctrip.Soa.Artemis.Common.Discovery;

namespace Com.Ctrip.Soa.Artemis.Client.Discovery
{
    [TestClass]
    public class ArtemisDiscoveryHttpClientTest
    {
        private static readonly ArtemisDiscoveryHttpClient _client
            = new ArtemisDiscoveryHttpClient(Constants.DiscoveryClientConfig);
        [TestMethod]
        public void TestGetService()
        {
            Service service = _client.GetService(new DiscoveryConfig()
            {
                ServiceId = Constants.RegistryServiceKey
            });
            Assert.AreEqual(Constants.RegistryServiceKey, service.ServiceId);
            Assert.IsNotNull(service.Instances);
            Assert.IsTrue(service.Instances.Count > 0);
        }

        [TestMethod]
        public void TestGetServices()
        {
            List<Service> services = _client.GetServices(new List<DiscoveryConfig>()
            {
                new DiscoveryConfig() {
                    ServiceId = Constants.RegistryServiceKey,
                },
                new DiscoveryConfig() {
                    ServiceId = Constants.RegistryServiceKey,
                }
            });
            Assert.AreEqual(2, services.Count);
            foreach (Service service in services) 
            {
                Assert.IsTrue(service.Instances.Count > 0);
            }
        }
    }
}
