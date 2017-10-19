using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Com.Ctrip.Soa.Artemis.Client.Test.Utils;

namespace Com.Ctrip.Soa.Artemis.Client.Common
{
    [TestClass]
    public class AddressManagerTest
    {

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            AddressManager.NewDiscoveryAddressManager("client", Constants.ManagerConfig);
            AddressManager.NewRegistryAddressManager("client", Constants.ManagerConfig);
        }

        [TestMethod]
        public void TestAvailableContext_ShouldReturnAvailableContext()
        {
            AddressManager manager = AddressManager.NewDiscoveryAddressManager("client", Constants.ManagerConfig);
            AddressContext ctx = manager.AddressContext;
            Assert.IsTrue(ctx.IsAvailable);
        }

        [TestMethod]
        [Timeout(5000)]
        public void TestAvailableContext_ShouldChangeAvailableContext()
        {
            AddressManager manager = AddressManager.NewDiscoveryAddressManager("client", Constants.ManagerConfig);
            AddressContext ctx = manager.AddressContext;
            Assert.IsTrue(ctx.IsAvailable);
            Assert.AreEqual(ctx, manager.AddressContext);
            ctx.MarkUnavailable();
            Assert.AreNotEqual(ctx, manager.AddressContext);
        }
    }
}
