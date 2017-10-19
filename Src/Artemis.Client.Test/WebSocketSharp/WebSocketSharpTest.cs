using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebSocketSharp;
using Com.Ctrip.Soa.Artemis.Client.Common;

namespace Com.Ctrip.Soa.Artemis.Client.WebSocketSharp
{
    [TestClass]
    public class WebSocketSharpTest
    {
        private const int times = 3;
        private const int interval = 3000;
        [TestMethod]
        public void TestDumpliacateConnections()
        {
            List<WebSocket> webSockets = new List<WebSocket>();
            int open = 0;
            int close = 0;
            for (int i = 0; i < times; i++)
            {
                WebSocket currentWebSocket = new WebSocket("ws://10.2.35.220:8080/artemis-service/ws/registry/heartbeat");
                webSockets.Add(currentWebSocket);
                currentWebSocket.OnOpen += (o, e) =>
                {
                    open++;
                };

                currentWebSocket.OnMessage += (o, e) =>
                {
                    Console.WriteLine(e.Data);
                };

                currentWebSocket.OnError += (o, e) =>
                {
                    Console.WriteLine(e);
                };

                currentWebSocket.OnClose += (o, e) =>
                {
                    close++;
                };

                currentWebSocket.Connect();

                Threads.Sleep(interval);

                if (currentWebSocket.IsAlive)
                {
                    currentWebSocket.Close();
                }
                else
                {
                    Assert.Fail("Close WebSocket failed");
                }
                Assert.IsFalse(currentWebSocket.IsAlive);
            }

            Threads.Sleep(interval);
            Assert.AreEqual(open, times);
            Assert.AreEqual(close, times);
        }
    }
}
