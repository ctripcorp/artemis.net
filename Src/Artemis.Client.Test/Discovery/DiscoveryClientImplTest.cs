using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Com.Ctrip.Soa.Artemis.Client.Test.Utils;
using Com.Ctrip.Soa.Artemis.Common;
using Com.Ctrip.Soa.Artemis.Common.Discovery;
using Com.Ctrip.Soa.Artemis.Client.Utils;

namespace Com.Ctrip.Soa.Artemis.Client.Discovery
{
    [TestClass]
    public class DiscoveryClientImplTest
    {
        private DiscoveryClientImpl _discoveryClientImpl = new DiscoveryClientImpl(Constants.ClientId, Constants.ManagerConfig);

        [TestMethod]
        public void TestGetService()
        {
            DiscoveryConfig discoveryConfig = new DiscoveryConfig()
            {
                ServiceId = Constants.NewServiceId()
            };
            Service service = _discoveryClientImpl.GetService(discoveryConfig);
            Assert.AreEqual(discoveryConfig.ServiceId, service.ServiceId);
        }

        [TestMethod]
        public void TestRegisterServiceChangeListener()
        {
            Dictionary<String, DefaultServiceChangeListener> registerServices = new Dictionary<string, DefaultServiceChangeListener>();
            string serviceId = Constants.NewServiceId();
            registerServices.Add("framework.soa.v1.registryservice", new DefaultServiceChangeListener());
            registerServices.Add("framework.soa.testservice.v2.testservice", new DefaultServiceChangeListener());
            registerServices.Add("framework.soa.test.v1.testportal", new DefaultServiceChangeListener());
            registerServices.Add(serviceId, new DefaultServiceChangeListener());

            foreach (string key in registerServices.Keys)
            {
                DiscoveryConfig discoveryConfig = new DiscoveryConfig()
                {
                    ServiceId = key
                };
                _discoveryClientImpl.RegisterServiceChangeListener(discoveryConfig, registerServices[key]);
            }

            foreach (string key in registerServices.Keys)
            {

                List<ServiceChangeEvent> serviceChangeEvents = registerServices[key].ServiceChangeEvents;
                Assert.AreEqual(0, serviceChangeEvents.Count);
            }
        }
    }
}
