using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Com.Ctrip.Soa.Artemis.Client.Test.Utils;

namespace Com.Ctrip.Soa.Artemis.Client.WebSocketSharp
{
    [TestClass]
    public class WebSocketSessionContextTest
    {
        private static volatile bool registryConnected = false;
        private static volatile bool discoveryConnected = false;
        public static List<WebSocketSessionContext> sessionContexts = new List<WebSocketSessionContext>()
        {
            new WebSocketSessionContext(Constants.DiscoveryClientConfig, 
                (webSocket) => 
                {
                    discoveryConnected = true; 
                }, 
                (webSocket, message) => 
                {
                }),

            new WebSocketSessionContext(Constants.RegistryClientConfig,
                 (webSocket) => 
                {
                    registryConnected = true; 
                }, 
                (webSocket, message) => 
                {
                }),

        };
        [TestMethod]
        public void TestConnect()
        {
            Assert.IsTrue(registryConnected);
            Assert.IsTrue(discoveryConnected);
            registryConnected = false;
            discoveryConnected = false;
            sessionContexts.ForEach(sessionContext =>
            {
                WebSocketSessionContext.Disconnect(sessionContext.Value);
            });
            Assert.IsTrue(registryConnected);
            Assert.IsTrue(discoveryConnected);
        }

        [TestMethod]
        public void TestExpired()
        {
            sessionContexts.ForEach(sessionContext => 
            {
                Assert.IsFalse(sessionContext.IsExpired);
            });
        }

        [TestMethod]
        public void TestValue()
        {
            sessionContexts.ForEach(sessionContext =>
            {
                Assert.IsNotNull(sessionContext.Value);
            });
        }
    }
}
