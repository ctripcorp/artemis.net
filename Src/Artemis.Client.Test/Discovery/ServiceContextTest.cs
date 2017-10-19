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
    public class ServiceContextTest
    {
        private static string _serviceId = Constants.NewServiceId();
        private DiscoveryConfig discoveryConfig = new DiscoveryConfig()
        {
            ServiceId = _serviceId
        };

        
        [TestMethod]
        public void TestInit()
        {
            ServiceContext ctx = new ServiceContext(discoveryConfig);
            Assert.IsNotNull(ctx.DiscoveryConfig);
            Assert.IsNotNull(ctx.newService());
            Assert.IsNotNull(ctx.GetListeners());
            Assert.IsNotNull(ctx.newServiceChangeEvent(InstanceChange.CHANGE_TYPE.RELOAD));
        }

        [TestMethod]
        public void testAddListener()
        {
            ServiceContext ctx = new ServiceContext(discoveryConfig);
            ServiceChangeListener listener1 = new DefaultServiceChangeListener();
            ctx.AddListener(listener1);
            Assert.AreEqual(1, ctx.GetListeners().Count);
            Assert.IsTrue(ctx.GetListeners().Contains(listener1));
            ServiceChangeListener listener2 = new DefaultServiceChangeListener();
            ctx.AddListener(listener2);
            Assert.AreEqual(2, ctx.GetListeners().Count);
            Assert.IsTrue(ctx.GetListeners().Contains(listener2));
        }

        [TestMethod]
        public void testDeleteInstance()
        {
            ServiceContext ctx = new ServiceContext(discoveryConfig);
            Instance instance1 = Constants.NewInstance(_serviceId);
            Assert.IsTrue(ctx.AddInstance(instance1));
            Assert.AreEqual(1, ctx.newService().Instances.Count);
            Assert.IsTrue(ctx.DeleteInstance(instance1));
            Assert.AreEqual(0, ctx.newService().Instances.Count);
            Assert.IsFalse(ctx.DeleteInstance(instance1));
        }

        [TestMethod]
        public void testAddInstance()
        {
            ServiceContext ctx = new ServiceContext(discoveryConfig);
            Instance instance1 = Constants.NewInstance(_serviceId);
            Assert.IsTrue(ctx.AddInstance(instance1));
            Assert.AreEqual(1, ctx.newService().Instances.Count);

            Instance instance2 = new Instance()
            {
                RegionId = instance1.RegionId,
                ZoneId = instance1.ZoneId,
                GroupId = instance1.GroupId,
                ServiceId = instance1.ServiceId.ToUpper(),
                InstanceId = instance1.InstanceId,
                MachineName = instance1.MachineName,
                IP = instance1.IP,
                Port = instance1.Port,
                Protocol = instance1.Protocol,
                Url = instance1.Url,
                HealthCheckUrl = instance1.HealthCheckUrl,
                Status = instance1.Status,
                Metadata = instance1.Metadata
            };
            Assert.IsTrue(ctx.AddInstance(instance2));
            Assert.AreEqual(1, ctx.newService().Instances.Count);

            Assert.IsTrue(ctx.AddInstance(Constants.NewInstance(_serviceId)));
            Assert.AreEqual(2, ctx.newService().Instances.Count);
        }


        [TestMethod]
        public void testIsAvailable()
        {
            ServiceContext ctx = new ServiceContext(discoveryConfig);
            Assert.IsFalse(ctx.IsAvailable());
            Instance instance = Constants.NewInstance(_serviceId);
            Assert.IsTrue(ctx.AddInstance(instance));
            Assert.AreEqual(1, ctx.newService().Instances.Count);
            Assert.IsTrue(ctx.IsAvailable());

            Assert.IsTrue(ctx.DeleteInstance(instance));
            Assert.IsFalse(ctx.IsAvailable());
        }
    }
}
