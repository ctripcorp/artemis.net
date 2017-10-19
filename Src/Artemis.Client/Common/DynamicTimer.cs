using System.Timers;
using Com.Ctrip.Soa.Artemis.Common.Configuration;
using Com.Ctrip.Soa.Caravan.Configuration;
using Com.Ctrip.Soa.Artemis.Client.Atomic;
using System;
using Com.Ctrip.Soa.Caravan.Logging;

namespace Com.Ctrip.Soa.Artemis.Client.Common
{
    public class DynamicTimer
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(DynamicTimer));
        private readonly Timer _timer;
        private readonly IProperty<int> _interval;
        private readonly AtomicBoolean _isRunning = new AtomicBoolean(false);
        private readonly Action _task;

        public DynamicTimer(IProperty<int> interval, Action task)
        {
            _interval = interval;
            _task = task;
            _timer = new Timer();
            _timer.Interval = _interval.Value;
            _timer.Enabled = true;
            _timer.AutoReset = true;
            _timer.Elapsed += new ElapsedEventHandler((o, e) =>
            {
                if (_isRunning.CompareAndSet(false, true)) 
                {
                    try
                    {
                        _task.Invoke();
                    }
                    catch (Exception ex)
                    {
                        _log.Warn("DynamicTimer run task failed", ex);
                    }
                    finally
                    {
                        _isRunning.Value = false;
                    }
                }
            });
            _interval.OnChange += new EventHandler<PropertyChangedEventArgs<int>>((o, arg) =>
            {
                _timer.Interval = _interval.Value;
            });
        }
    }
}
