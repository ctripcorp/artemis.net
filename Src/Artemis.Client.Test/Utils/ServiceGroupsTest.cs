using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Com.Ctrip.Soa.Artemis.Common.Util;

namespace Com.Ctrip.Soa.Artemis.Client.Utils
{
    [TestClass]
    public class ServiceGroupsTest
    {
        [TestMethod]
        public void TestFixWeight()
        {
            Assert.AreEqual(ServiceGroups.DEFAULT_WEIGHT_VALUE, ServiceGroups.FixWeight(null));
            Assert.AreEqual(ServiceGroups.MIN_WEIGHT_VALUE, ServiceGroups.FixWeight(ServiceGroups.MIN_WEIGHT_VALUE));
            Assert.AreEqual(ServiceGroups.MAX_WEIGHT_VALUE, ServiceGroups.FixWeight(ServiceGroups.MAX_WEIGHT_VALUE));

            Assert.AreEqual(ServiceGroups.MIN_WEIGHT_VALUE + 1, ServiceGroups.FixWeight(ServiceGroups.MIN_WEIGHT_VALUE + 1));
            Assert.AreEqual(ServiceGroups.MAX_WEIGHT_VALUE - 1, ServiceGroups.FixWeight(ServiceGroups.MAX_WEIGHT_VALUE - 1));

            Assert.AreEqual(ServiceGroups.DEFAULT_WEIGHT_VALUE, ServiceGroups.FixWeight(ServiceGroups.MIN_WEIGHT_VALUE - 1));
            Assert.AreEqual(ServiceGroups.MAX_WEIGHT_VALUE, ServiceGroups.FixWeight(ServiceGroups.MAX_WEIGHT_VALUE + 1));
        }
    }
}
