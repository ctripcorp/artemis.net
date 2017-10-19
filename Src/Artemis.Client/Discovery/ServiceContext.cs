using Com.Ctrip.Soa.Artemis.Client.Atomic;
using System.Collections.Generic;
using System.Linq;
using Com.Ctrip.Soa.Artemis.Common;
using Com.Ctrip.Soa.Artemis.Common.Condition;
using Com.Ctrip.Soa.Artemis.Common.Discovery;
using System.Runtime.CompilerServices;
using Com.Ctrip.Soa.Artemis.Common.Util;

namespace Com.Ctrip.Soa.Artemis.Client.Discovery
{
    /// <summary>
    /// Created by fang_j 2016/08/04.
    /// </summary>
    public class ServiceContext
    {
        private readonly DiscoveryConfig _discoveryConfig;
        private readonly string _serviceId;
        private readonly HashSet<ServiceChangeListener> _listeners;
        private volatile Service _service;

        public ServiceContext(DiscoveryConfig discoveryConfig)
        {
            Preconditions.CheckArgument(discoveryConfig != null, "discoveryConfig");
            Preconditions.CheckArgument(!string.IsNullOrWhiteSpace(discoveryConfig.ServiceId), "serviceId");
            _discoveryConfig = discoveryConfig;
            _serviceId = discoveryConfig.ServiceId.ToLower();
            _listeners = new HashSet<ServiceChangeListener>();
            _service = new Service()
            {
                ServiceId = discoveryConfig.ServiceId
            };
        }

        public DiscoveryConfig DiscoveryConfig
        {
            get { return _discoveryConfig; }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void SetService(Service service)
        {
            Preconditions.CheckArgument(service != null, "service");
            string newServiceId = service.ServiceId;
            Preconditions.CheckArgument(!string.IsNullOrWhiteSpace(newServiceId), "serviceId");
            Preconditions.CheckArgument(string.Equals(_serviceId, newServiceId.ToLower()),
                "service's serviceId is not this same as discoveryConfig. expected: " + _serviceId + ", actual: " + newServiceId);
            _service = service;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool IsAvailable()
        {
            Service service = _service;
            return service != null && !Conditions.IsNullOrEmpty(service.Instances);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public HashSet<ServiceChangeListener> GetListeners()
        {
            return new HashSet<ServiceChangeListener>(_listeners);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool UpdateInstance(Instance instance)
        {
            return AddInstance(instance);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool AddInstance(Instance instance)
        {
            if (instance == null)
            {
                return false;
            }

            string instanceServiceId = instance.ServiceId;
            if (instanceServiceId == null || !_serviceId.Equals(instanceServiceId.ToLower()))
            {
                return false;
            }

            List<Instance> instances = _service.Instances;
            if (instances == null)
            {
                instances = new List<Instance>();
            }
            DeleteInstance(instance);
            instances.Add(instance);
            _service.Instances = instances;
            return true;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool DeleteInstance(Instance instance)
        {
            if (instance == null)
            {
                return false;
            }

            if (_service.Instances == null)
            {
                return false;
            }

            return _service.Instances.Remove(instance);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void AddListener(ServiceChangeListener listener)
        {
            if (listener == null)
            {
                return;
            }
            _listeners.Add(listener);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public Service newService()
        {
            return new Service()
            {
                ServiceId = _serviceId,
                Instances = _service.Instances == null ? null : new List<Instance>(_service.Instances),
                LogicInstances = _service.LogicInstances == null ? null : new List<Instance>(_service.LogicInstances),
                Metadata = _service.Metadata == null ? null : new Dictionary<string, string>(_service.Metadata),
                RouteRules = _service.NewRouteRules()
            };
        }

        public ServiceChangeEvent newServiceChangeEvent(string changeType)
        {
            Preconditions.CheckArgument(!string.IsNullOrWhiteSpace(changeType), "changeType");
            Service service = newService();
            return new ServiceChangeEventImpl()
            {
                ChangeType = changeType,
                ChangedService = service
            };
        }

        internal class ServiceChangeEventImpl : ServiceChangeEvent
        {
            public string ChangeType { get; set; }

            public Service ChangedService { get; set; }
        }

        public override string ToString()
        {
            return _serviceId;
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

            if (obj.GetType() != typeof(Service))
            {
                return false;
            }

            return string.Equals(ToString(), obj.ToString());
        }
    }
}
