using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Com.Ctrip.Soa.Artemis.Client.Utils
{
    public class DefaultServiceChangeListener : ServiceChangeListener
    {
        private readonly List<ServiceChangeEvent> _serviceChangeEvents = new List<ServiceChangeEvent>();

        public void OnChange(ServiceChangeEvent serviceChangeEvent)
        {
            _serviceChangeEvents.Add(serviceChangeEvent);
        }

        public List<ServiceChangeEvent> ServiceChangeEvents
        {
            get
            {
                return _serviceChangeEvents;
            }
        }
    }
}
