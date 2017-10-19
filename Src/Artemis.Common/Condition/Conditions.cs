using System.Collections.Generic;

namespace Com.Ctrip.Soa.Artemis.Common.Condition
{
    public class Conditions
    {
        public static bool IsNullOrEmpty<T>(List<T> list)
        {
            return list == null || list.Count == 0;
        }
    }
}
