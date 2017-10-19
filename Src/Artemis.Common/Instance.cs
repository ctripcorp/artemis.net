using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Com.Ctrip.Soa.Artemis.Common
{
    public class Instance
    {
        public class STATUS
        {

            public static string STARTING = "starting";
            public static string UP = "up";
            public static string DOWN = "down";
            public static string UNHEALTHY = "unhealthy";
            public static string UNKNOWN = "unknown";
        }

        public string RegionId { get; set; }
        public string ZoneId { get; set; }
        public string GroupId { get; set; }
        public string ServiceId { get; set; }
        public string InstanceId { get; set; }
        public string MachineName { get; set; }
        public string IP { get; set; }
        public int Port { get; set; }
        public string Protocol { get; set; }
        public string Url { get; set; }
        public string HealthCheckUrl { get; set; }
        public string Status { get; set; }
        public Dictionary<string, string> Metadata { get; set; }

        public override string ToString()
        {
            string res = RegionId + "/" + ZoneId + "/" + ServiceId;
            if (!string.IsNullOrWhiteSpace(GroupId))
            {
                res += "/" + GroupId;
            }
            res += "/" + InstanceId;
            return res.ToLower();
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj.GetType() != typeof(Instance))
            {
                return false;
            }

            return string.Equals(ToString(), obj.ToString());
        }

        public static bool IsValid (Instance instance)
        {
            return instance != null
                && !string.IsNullOrWhiteSpace(instance.InstanceId)
                && !string.IsNullOrWhiteSpace(instance.ServiceId)
                && !string.IsNullOrWhiteSpace(instance.Url);
        }
    }
}
