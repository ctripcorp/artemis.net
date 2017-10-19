using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Com.Ctrip.Soa.Artemis.Client.Test.Utils;
using Com.Ctrip.Soa.Artemis.Client.Common;
using Com.Ctrip.Soa.Artemis.Common;

namespace Com.Ctrip.Soa.Artemis.Client.Registry
{
    [TestClass]
    public class InstanceRepositoryTest
    {
        private static InstanceRepository _instanceRepository = new InstanceRepository(Constants.DiscoveryClientConfig);

        [TestMethod]
        public void TestUpdate()
        {
            Instance[] instances = new Instance[]{Constants.NewInstance(), Constants.NewInstance()};
            _instanceRepository.Update(instances, RegisterType.register);
            Assert.AreEqual(instances.Length, _instanceRepository.AvailableInstances.Count);
            _instanceRepository.Update(instances, RegisterType.register);
            Assert.AreEqual(instances.Length, _instanceRepository.AvailableInstances.Count);

            _instanceRepository.Update(instances, RegisterType.unregister);
            Assert.AreEqual(0, _instanceRepository.AvailableInstances.Count);
        }
    }
}
