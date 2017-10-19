using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Com.Ctrip.Soa.Artemis.Client.Test.Utils;
using Com.Ctrip.Soa.Artemis.Common;
using Com.Ctrip.Soa.Artemis.Client.Utils;
using Com.Ctrip.Soa.Artemis.Common.Discovery;
using Com.Ctrip.Soa.Artemis.Client.Common;
using Com.Ctrip.Soa.Artemis.Client.Atomic;
using Com.Ctrip.Soa.Artemis.Client.Registry;

namespace Com.Ctrip.Soa.Artemis.Client.Discovery
{
    [TestClass]
    public class ServiceDiscoveryTest
    {
        [TestMethod]
        public void TestReload()
        {
            ServiceRepository serviceRepository = new ServiceRepository(Constants.DiscoveryClientConfig);
            List<Service> services = new List<Service>();

            Assert.AreEqual(0, services.Count);
            HashSet<String> serviceKeys = new HashSet<String>(){
                Constants.RegistryServiceKey,Constants.JavaRegistryServiceKey
            };
            Dictionary<String, ServiceChangeListener> serviceChangeListeners = new Dictionary<string, ServiceChangeListener>();
            
            foreach (String serviceKey in serviceKeys)
            {
                DefaultServiceChangeListener listener = new DefaultServiceChangeListener();
                DiscoveryConfig discoveryConfig = new DiscoveryConfig()
                {
                    ServiceId = serviceKey
                };
                serviceChangeListeners.Add(serviceKey, listener);
                serviceRepository.RegisterServiceChangeListener(discoveryConfig, listener);
                serviceRepository._serviceDiscovery.Reload(discoveryConfig);
                Threads.Sleep(100);
                Assert.IsTrue(listener.ServiceChangeEvents.Count >= 1);
                foreach (ServiceChangeEvent e in listener.ServiceChangeEvents)
                {
                    Assert.AreEqual(InstanceChange.CHANGE_TYPE.RELOAD, e.ChangeType);
                    Assert.AreEqual(serviceKey, e.ChangedService.ServiceId);
                }
            }
        }

        [TestMethod]
        public void testSubscribe()
        {
            ServiceRepository serviceRepository = new ServiceRepository(Constants.DiscoveryClientConfig);
            String serviceId = Constants.NewServiceId();
            DiscoveryConfig discoveryConfig = new DiscoveryConfig()
            {
                ServiceId = serviceId
            };
            Instance[] instances = new Instance[]
            {
                Constants.NewInstance(serviceId),
                Constants.NewInstance(serviceId)
            };

            CustomServiceChangeListener customServiceChangeListener = new CustomServiceChangeListener();

            serviceRepository.RegisterServiceChangeListener(discoveryConfig, customServiceChangeListener);
            Threads.Sleep(2000); // wait service discovery websocket session created.

            InstanceRepository instanceRepository = new InstanceRepository(Constants.RegistryClientConfig);
            instanceRepository.Register(instances);
            Assert.IsTrue(customServiceChangeListener.AwaitAdd(2, 5000));
            instanceRepository.Unregister(instances);
            Assert.IsTrue(customServiceChangeListener.AwaitDelete(2, 5000));
            Assert.IsTrue(2 * instances.Length <= customServiceChangeListener.ServiceChangeEvents.Count);
        }


        public class CustomServiceChangeListener : ServiceChangeListener
        {
            private List<ServiceChangeEvent> serviceChangeEvents = new List<ServiceChangeEvent>();
            private AtomicLong addCount = new AtomicLong(0);
            private AtomicLong deleteCount = new AtomicLong(0);

            public List<ServiceChangeEvent> ServiceChangeEvents
            {
                get { return serviceChangeEvents; }
            }

            public long AddCount
            {
                get { return addCount.Value; }
            }

            public long DeleteCount
            {
                get { return deleteCount.Value; }
            }

            public bool AwaitAdd(long value, long timeout)
            {
                long start = DateTimeUtils.CurrentTimeInMilliseconds;
                while (addCount.Value < value)
                {
                    if (start + timeout <= DateTimeUtils.CurrentTimeInMilliseconds)
                    {
                        throw new TimeoutException();
                    }
                    Threads.Sleep(100);
                }
                return addCount.Value == value;
            }

            public bool AwaitDelete(long value, long timeout)
            {
                long start = DateTimeUtils.CurrentTimeInMilliseconds;
                while (deleteCount.Value < value)
                {
                    if (start + timeout <= DateTimeUtils.CurrentTimeInMilliseconds)
                    {
                        throw new TimeoutException();
                    }
                    Threads.Sleep(100);
                }
                return deleteCount.Value == value;
            }

            public void OnChange(ServiceChangeEvent serviceChangeEvent)
            {
                serviceChangeEvents.Add(serviceChangeEvent);
                if (InstanceChange.CHANGE_TYPE.DELETE.Equals(serviceChangeEvent.ChangeType))
                {
                    deleteCount.IncrementAndGet();
                }

                if (InstanceChange.CHANGE_TYPE.NEW.Equals(serviceChangeEvent.ChangeType))
                {
                    addCount.IncrementAndGet();
                }
            }
        }
    }
}
