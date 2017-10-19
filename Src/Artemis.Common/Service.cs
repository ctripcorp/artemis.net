using System.Collections.Generic;
using System;
using Com.Ctrip.Soa.Artemis.Common.Text;

namespace Com.Ctrip.Soa.Artemis.Common
{
    public class Service
    {
        public string ServiceId { get; set; }
        public Dictionary<string, string> Metadata { get; set; }
        public List<Instance> Instances { get; set; }
        public List<Instance> LogicInstances { get; set; }
        public List<RouteRule> RouteRules { get; set; }

        public override string ToString()
        {
            return ServiceId;
        }

        public override int GetHashCode()
        {
            string serviceId = ServiceId;
            return serviceId == null ? 0 : serviceId.ToLower().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj.GetType() != typeof(Service))
            {
                return false;
            }

            string serviceId = ServiceId == null ? null : ServiceId.ToLower();
            string otherServiceId = ((Service)obj).ServiceId;
            otherServiceId = otherServiceId == null ? null : otherServiceId.ToLower();

            return string.Equals(serviceId, otherServiceId);
        }

        public static bool IsValidService (Service service)
        {
            return service != null && !string.IsNullOrWhiteSpace(service.ServiceId);
        }

        public static bool IsValidServices (List<Service>services)
        {
            if (services == null || services.Count == 0)
            {
                return false;
            }
            foreach (Service service in services)
            {
                if (!Service.IsValidService(service))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
