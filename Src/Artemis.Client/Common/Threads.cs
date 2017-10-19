using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Com.Ctrip.Soa.Caravan.Logging;
using System.Threading;

namespace Com.Ctrip.Soa.Artemis.Client.Common
{
    public class Threads
    {
        private static int _seed = Environment.TickCount;
        public static readonly ThreadLocal<Random> _random =
            new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref _seed)));
        private static readonly ILog _log = LogManager.GetLogger(typeof(Threads));

        public static void Sleep(int millisecondsTimeout)
        {
            try
            {
                Thread.Sleep(millisecondsTimeout);
            }
            catch (Exception e)
            {
                _log.Error("Sleep failed", e);
            }
        }

        public static Random ThreadLocalRandom
        {
            get { return _random.Value; }
        }
    }
}
