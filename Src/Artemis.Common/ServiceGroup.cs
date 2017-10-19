using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Com.Ctrip.Soa.Artemis.Common
{
    public class ServiceGroup
    {
        public string GroupKey { get; set; }
        public int? Weight { get; set; }
        public List<Instance> Instances { get; set; }
        public Dictionary<string, string> Metadata { get; set; }
        public List<String> InstanceIds { get; set; }
    }
}
