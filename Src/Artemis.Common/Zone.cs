using System.Collections.Generic;

namespace Com.Ctrip.Soa.Artemis.Common
{
    public class Zone
    {
        public string RegionId { get; set; }
        public string ZoneId { get; set; }
        public Dictionary<string, string> Metadata { get; set; }

        public override string ToString()
        {
            return (RegionId + "/" + ZoneId).ToLower();
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

            if (obj.GetType() != typeof(Zone))
            {
                return false;
            }

            return string.Equals(ToString(), obj.ToString());
        }
    }
}
