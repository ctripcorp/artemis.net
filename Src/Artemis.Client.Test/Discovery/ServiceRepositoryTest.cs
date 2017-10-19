using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Com.Ctrip.Soa.Artemis.Client.Test.Utils;
using Com.Ctrip.Soa.Artemis.Client.Common;
using System.Threading;
using Com.Ctrip.Soa.Artemis.Common;
using Com.Ctrip.Soa.Artemis.Common.Discovery;

namespace Com.Ctrip.Soa.Artemis.Client.Discovery
{
    [TestClass]
    public class ServiceRepositoryTest
    {
        private readonly ServiceRepository _serviceRepository = new ServiceRepository(Constants.DiscoveryClientConfig);

        [TestMethod]
        public void TestGetService()
        {
            DiscoveryConfig discoveryConfig = Constants.NewDiscoveryConfig();
            string serviceId = discoveryConfig.ServiceId;
            Service service = _serviceRepository.GetService(discoveryConfig);
            Assert.AreEqual(serviceId, service.ServiceId);
            Assert.IsTrue(_serviceRepository.ContainsService(serviceId));
        }

        [TestMethod]
        public void TestRegisterServiceChangeListener()
        {
            DiscoveryConfig discoveryConfig = Constants.NewDiscoveryConfig();
            _serviceRepository.RegisterServiceChangeListener(discoveryConfig, new DefaultServiceChangeListener());
            Assert.IsTrue(_serviceRepository.ContainsService(discoveryConfig.ServiceId));
        }

        [TestMethod]
        public void TestUpdate_Instance()
        {
            for (int i = 0; i < 5; i++)
            {
                DiscoveryConfig discoveryConfig = Constants.NewDiscoveryConfig();
                string serviceId = discoveryConfig.ServiceId;
                List<DefaultServiceChangeListener> listeners = new List<DefaultServiceChangeListener>()
            {
                new DefaultServiceChangeListener(),
                new DefaultServiceChangeListener(),
                new DefaultServiceChangeListener(),
                new DefaultServiceChangeListener()
            };

                foreach (DefaultServiceChangeListener listener in listeners)
                {
                    _serviceRepository.RegisterServiceChangeListener(discoveryConfig, listener);
                }

                Assert.IsTrue(_serviceRepository.ContainsService(serviceId));
                string[] changeTypes = new string[] 
                {
                    InstanceChange.CHANGE_TYPE.DELETE, InstanceChange.CHANGE_TYPE.NEW, InstanceChange.CHANGE_TYPE.NEW,
                    InstanceChange.CHANGE_TYPE.NEW, InstanceChange.CHANGE_TYPE.DELETE, InstanceChange.CHANGE_TYPE.DELETE,
                    InstanceChange.CHANGE_TYPE.CHANGE, InstanceChange.CHANGE_TYPE.CHANGE
                };

                Instance context = Constants.NewInstance(serviceId);
                Instance changeContext = Constants.NewInstance(serviceId);

                foreach (string changeType in changeTypes)
                {
                    if (InstanceChange.CHANGE_TYPE.CHANGE.Equals(changeType))
                    {
                        _serviceRepository.Update(Constants.newInstanceChange(changeType, changeContext));
                    }
                    else
                    {
                        _serviceRepository.Update(Constants.newInstanceChange(changeType, context));
                    }
                }

                Threads.Sleep(100);
                foreach (DefaultServiceChangeListener listener in listeners)
                {
                    Assert.AreEqual(6, listener.ServiceChangeEvents.Count);

                    ServiceChangeEvent add1 = listener.ServiceChangeEvents[0];
                    ServiceChangeEvent add2 = listener.ServiceChangeEvents[1];
                    ServiceChangeEvent add3 = listener.ServiceChangeEvents[2];
                    ServiceChangeEvent delete = listener.ServiceChangeEvents[3];
                    ServiceChangeEvent change1 = listener.ServiceChangeEvents[4];
                    ServiceChangeEvent change2 = listener.ServiceChangeEvents[5];

                    Assert.AreEqual(add1.ChangeType, InstanceChange.CHANGE_TYPE.NEW);
                    Assert.AreEqual(add2.ChangeType, InstanceChange.CHANGE_TYPE.NEW);
                    Assert.AreEqual(add3.ChangeType, InstanceChange.CHANGE_TYPE.NEW);
                    Assert.AreEqual(delete.ChangeType, InstanceChange.CHANGE_TYPE.DELETE);
                    Assert.AreEqual(change1.ChangeType, InstanceChange.CHANGE_TYPE.CHANGE);
                    Assert.AreEqual(change1.ChangeType, InstanceChange.CHANGE_TYPE.CHANGE);

                    Assert.IsTrue(new HashSet<Instance>(add1.ChangedService.Instances).Contains(context));
                    Assert.IsTrue(new HashSet<Instance>(add2.ChangedService.Instances).Contains(context));
                    Assert.IsTrue(new HashSet<Instance>(add2.ChangedService.Instances).Contains(context));
                    Assert.IsTrue(delete.ChangedService.Instances.Count == 0);
                    Assert.IsTrue(new HashSet<Instance>(change1.ChangedService.Instances).Contains(changeContext));
                    Assert.IsTrue(new HashSet<Instance>(change1.ChangedService.Instances).Contains(changeContext));
                }
            }
        }

        [TestMethod]
        public void TestUpdate_Service()
        {
            for (int i = 0; i < 5; i++)
            {
                DiscoveryConfig discoveryConfig = Constants.NewDiscoveryConfig();
                string serviceId = discoveryConfig.ServiceId;
                DefaultServiceChangeListener listener = new DefaultServiceChangeListener();
                _serviceRepository.RegisterServiceChangeListener(discoveryConfig, listener);
                Assert.IsTrue(_serviceRepository.ContainsService(serviceId));

                Service service1 = new Service()
                {
                    ServiceId = serviceId,
                    Instances = new List<Instance>()
                    {
                        Constants.NewInstance(serviceId)
                    }
                };

                Service service2 = new Service()
                {
                    ServiceId = serviceId,
                    Instances = new List<Instance>()
                    {
                        Constants.NewInstance(serviceId), Constants.NewInstance(serviceId)
                    }
                };


                _serviceRepository.Update(service1);
                _serviceRepository.Update(service2);
                _serviceRepository.Update(new Service()
                {
                    ServiceId = serviceId + "1"
                });

                Threads.Sleep(100);
                Assert.AreEqual(2, listener.ServiceChangeEvents.Count);
                foreach (ServiceChangeEvent e in listener.ServiceChangeEvents)
                {
                    Assert.AreEqual(InstanceChange.CHANGE_TYPE.RELOAD, e.ChangeType);
                }
            }
        }

        public class DefaultServiceChangeListener : ServiceChangeListener
        {
            private readonly List<ServiceChangeEvent> _serviceChangeEvents = new List<ServiceChangeEvent>();

            public void OnChange(ServiceChangeEvent serviceChangeEvent)
            {
                _serviceChangeEvents.Add(serviceChangeEvent);
            }

            public List<ServiceChangeEvent> ServiceChangeEvents
            {
                get
                {
                    return _serviceChangeEvents;
                }
            }
        }
    }
}
