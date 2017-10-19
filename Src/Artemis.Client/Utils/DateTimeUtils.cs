using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Com.Ctrip.Soa.Artemis.Client.Utils
{
    internal static class DateTimeUtils
    {
        public static long CurrentTimeInMilliseconds
        {
            get
            {
                return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            }
        }
    }
}
