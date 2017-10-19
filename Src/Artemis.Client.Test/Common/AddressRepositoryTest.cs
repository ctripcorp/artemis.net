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
    public class AddressRepositoryTest
    {
        [TestMethod]
        [Timeout(5000)]
        public void TestAddress()
        {
            AddressRepository addressRepository = new AddressRepository(Constants.ClientId, Constants.ManagerConfig, RestPaths.CLUSTER_UP_DISCOVERY_NODES_FULL_PATH);
            string address = addressRepository.Address;
            while (address == null)
            {
                address = addressRepository.Address;
                Threads.Sleep(100);
            }
           Assert.IsNotNull(address);
        }
    }
}
