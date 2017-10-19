using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Com.Ctrip.Soa.Artemis.Common.Text;

namespace Com.Ctrip.Soa.Artemis.Common
{
    public class RouteRule
    {
        public class STRATEGY
        {
            public const string WEIGHTED_ROUND_ROBIN = "weighted-round-robin";
            public const string CLOSE_BY_VISIT = "close-by-visit";
        }
        public string RouteId { get; set; }
        public String Strategy { get; set; }
        public List<ServiceGroup> Groups { get; set; }

        public override string ToString()
        {
            return this.ToJson();
        }
    }
}
