using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Com.Ctrip.Soa.Artemis.Client.Test.Utils;
using Com.Ctrip.Soa.Artemis.Common.Configuration;

namespace Com.Ctrip.Soa.Artemis.Client.Common
{
    [TestClass]
    public class AddressContextTest
    {
        private const string wsEndpoint = "/endpoint";
        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            AddressContext ctx = new AddressContext(Constants.ClientId, Constants.ManagerConfig);
            Assert.IsFalse(ctx.IsAvailable);
            Assert.IsFalse(ctx.IsExpired);
        }

        [TestMethod]
        public void TestGetHttpUrl()
        {
            AddressContext context = new AddressContext(Constants.ClientId, Constants.ManagerConfig, "http://localhost:8080", wsEndpoint);
            Assert.AreEqual("http://localhost:8080", context.HttpUrl);
            Assert.AreEqual("http://localhost:8080/path", context.CustomHttpUrl("path"));
        }

        [TestMethod]
        public void TestGetWebSocketEndPoint()
        {
            AddressContext context = new AddressContext(Constants.ClientId, Constants.ManagerConfig, "http://localhost:8080", wsEndpoint);
            Assert.AreEqual("ws://localhost:8080" + wsEndpoint, context.WebSocketEndpoint);
        }
    }
}