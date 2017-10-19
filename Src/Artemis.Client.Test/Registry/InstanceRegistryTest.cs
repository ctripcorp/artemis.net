using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Com.Ctrip.Soa.Artemis.Client.Test.Utils;
using Com.Ctrip.Soa.Artemis.Common;
using Com.Ctrip.Soa.Artemis.Client.Common;
using Com.Ctrip.Soa.Artemis.Common.Registry;
using WebSocketSharp;

namespace Com.Ctrip.Soa.Artemis.Client.Registry
{
    [TestClass]
    public class InstanceRegistryTest
    {
        [TestMethod]
        [Timeout(20000)]
        public void TestRegister()
        {
            InstanceRepository repository = new InstanceRepository(Constants.RegistryClientConfig);
            Instance[] instances = new Instance[] { Constants.NewInstance(), Constants.NewInstance() };
            repository.Update(instances, RegisterType.register);
            CustomInstanceRegistry registry = new CustomInstanceRegistry(repository, Constants.RegistryClientConfig);
            while (registry.FailedInstances.Count == 0)
            {
                Threads.Sleep(10);
            }
            HashSet<Instance> instancesSet = new HashSet<Instance>();
            registry.FailedInstances.ForEach(failedInstance =>
            {
                instancesSet.Add(failedInstance.Instance);
            });
            Assert.AreEqual(instancesSet.Count, instances.Length);
            foreach (Instance instance in instances)
            {
                Assert.IsTrue(instancesSet.Contains(instance));
            }
            while (registry.AcceptHeartbeats < 1 || registry.SendHeartbeats < 1)
            {
                Threads.Sleep(500);
            }
        }

        [TestMethod]
        [Timeout(20000)]
        public void TestDynamicInstanceRegister()
        {
            InstanceRepository repository = new InstanceRepository(Constants.RegistryClientConfig);
            CustomInstanceRegistry registry = new CustomInstanceRegistry(repository, Constants.RegistryClientConfig);
            
            Instance[] instances = new Instance[] { Constants.NewInstance(), Constants.NewInstance() };
            repository.Update(instances, RegisterType.register);
            HashSet<Instance> instancesSet = new HashSet<Instance>();
            while (registry.FailedInstances.Count == 0)
            {
                Threads.Sleep(10);
            }
            registry.FailedInstances.ForEach(failedInstance =>
            {
                instancesSet.Add(failedInstance.Instance);
            });
            Assert.AreEqual(instancesSet.Count, instances.Length);
            foreach (Instance instance in instances)
            {
                Assert.IsTrue(instancesSet.Contains(instance));
            }
            while (registry.AcceptHeartbeats < 1 || registry.SendHeartbeats < 1)
            {
                Threads.Sleep(500);
            }
        }
    }

    public class CustomInstanceRegistry : InstanceRegistry
    {
        private readonly List<FailedInstance> _failedInstances = new List<FailedInstance>();
        private volatile int sendHearts = 0;
        private volatile int acceptHearts = 0;
        public CustomInstanceRegistry(InstanceRepository instanceRepository, ArtemisClientConfig config)
            :base(instanceRepository, config)
        {
        }

        public int SendHeartbeats
        {
            get { return sendHearts; }
        }

        public int AcceptHeartbeats
        {
            get { return acceptHearts; }
        }

        public List<FailedInstance> FailedInstances
        {
            get { return _failedInstances; }
        }

        public override void Register(List<FailedInstance> failedInstances)
        {
            base.Register(failedInstances);
            _failedInstances.AddRange(failedInstances);
        }

        protected override void AcceptHeartbeat(MessageEventArgs message)
        {
            base.AcceptHeartbeat(message);
            acceptHearts++;
        }

        protected override void SendHeartbeat()
        {
            base.SendHeartbeat();
            sendHearts++;
        }
    }
}
