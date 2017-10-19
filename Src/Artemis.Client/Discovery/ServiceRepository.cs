using System.Collections.Concurrent;
using Com.Ctrip.Soa.Artemis.Client.Common;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Com.Ctrip.Soa.Artemis.Client.Atomic;
using Com.Ctrip.Soa.Artemis.Common.Configuration;
using Com.Ctrip.Soa.Artemis.Common.Condition;
using Com.Ctrip.Soa.Artemis.Common;
using Com.Ctrip.Soa.Artemis.Common.Util;
using Com.Ctrip.Soa.Artemis.Common.Discovery;
using Com.Ctrip.Soa.Caravan.Metric;
using Com.Ctrip.Soa.Artemis.Common.Text;

using Com.Ctrip.Soa.Caravan.Logging;

namespace Com.Ctrip.Soa.Artemis.Client.Discovery
{
    public class ServiceRepository
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(ServiceRepository));
        private readonly ConcurrentDictionary<string, ServiceContext> _services =
            new ConcurrentDictionary<string, ServiceContext>();
        private readonly ConcurrentDictionary<string, DiscoveryConfig> _discoveryConfigs =
            new ConcurrentDictionary<string, DiscoveryConfig>();
        private readonly ConcurrentQueue<Action> _serviceChangeNotifies = new ConcurrentQueue<Action>();
        private readonly IEventMetricManager _eventMetricManager;
        private readonly string _serviceDiscoveryMetricName;
        internal readonly ServiceDiscovery _serviceDiscovery;

        public ServiceRepository(ArtemisClientConfig config)
        {
            Preconditions.CheckArgument(config != null, "config");
            _eventMetricManager = config.EventMetricManager;
            _serviceDiscoveryMetricName = config.Key("service-discovery.instance-change.event.distribution");
            _serviceDiscovery = new ServiceDiscovery(this, config);
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    Action notify = null;
                    while (_serviceChangeNotifies.TryDequeue(out notify))
                    {
                        notify();
                    }
                    Threads.Sleep(10);
                }
            }, TaskCreationOptions.LongRunning);
        }

        public virtual List<DiscoveryConfig> DiscoveryConfigs
        {
            get
            {
                return _services.Values.Select(serviceContext => serviceContext.DiscoveryConfig).ToList();
            }
        }

        public ICollection<ServiceContext> ServiceContexts
        {
            get
            {
                return _services.Values;
            }
        }

        public DiscoveryConfig GetDiscoveryConfig(string serviceId)
        {
            if (string.IsNullOrWhiteSpace(serviceId))
            {
                return null;
            }
            return _discoveryConfigs[serviceId.ToLower()];
        }

        public virtual bool ContainsService(string serviceId)
        {
            if (string.IsNullOrWhiteSpace(serviceId))
            {
                return false;
            }
            return _services.ContainsKey(serviceId.ToLower());
        }

        public Service GetService(DiscoveryConfig discoveryConfig)
        {
            if (discoveryConfig == null)
            {
                return new Service();
            }

            string serviceId = discoveryConfig.ServiceId;
            if (string.IsNullOrWhiteSpace(serviceId))
            {
                return new Service();
            }
            if (!this.ContainsService(serviceId))
            {
                RegisterService(discoveryConfig);
            }
            return _services[serviceId.ToLower()].newService();
        }

        public void RegisterServiceChangeListener(DiscoveryConfig discoveryConfig, ServiceChangeListener listener)
        {
            if (discoveryConfig == null || listener == null)
            {
                return;
            }

            string serviceId = discoveryConfig.ServiceId;
            if (string.IsNullOrWhiteSpace(serviceId))
            {
                return;
            }

            if (!this.ContainsService(serviceId))
            {
                RegisterService(discoveryConfig);
            }

            _services[serviceId.ToLower()].AddListener(listener);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void RegisterService(DiscoveryConfig discoveryConfig)
        {
            if (discoveryConfig == null)
            {
                return;
            }

            string serviceId = discoveryConfig.ServiceId;
            if (string.IsNullOrWhiteSpace(serviceId) || ContainsService(serviceId))
            {
                return;
            }
            serviceId = serviceId.ToLower();

            if (ContainsService(serviceId))
                return;

            ServiceContext serviceContext = new ServiceContext(discoveryConfig);
            try
            {
                Service service = _serviceDiscovery.GetService(discoveryConfig);
                serviceContext.SetService(service);
            }
            catch (Exception e)
            {
                _log.Error("init service failed", e);
            }

            _services[serviceId] = serviceContext;
            _discoveryConfigs[serviceId] = discoveryConfig;
            try
            {
                _serviceDiscovery.RegisterDiscoveryConfig(discoveryConfig);
            }
            catch (Exception e)
            {
                _log.Warn(string.Format("register the service {0} to the {1} failed", serviceId, _serviceDiscovery), e);
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public virtual void Update(Service service)
        {
            try
            {
                if (service == null)
                {
                    return;
                }

                string serviceId = service.ServiceId;
                if (serviceId == null)
                {
                    return;
                }

                ServiceContext currentContext = _services[serviceId.ToLower()];
                if (currentContext == null)
                {
                    return;
                }

                currentContext.SetService(service);
                ServiceChangeEvent serviceChangeEvent = currentContext.newServiceChangeEvent(InstanceChange.CHANGE_TYPE.RELOAD);
                foreach (ServiceChangeListener listener in currentContext.GetListeners())
                {
                    NotifyServiceChange(listener, serviceChangeEvent);
                }

                _log.Info("Operation:" + serviceChangeEvent.ChangeType + "\n" + "service: " + service.ToJson());
                Metric(serviceChangeEvent.ChangeType, true, service);
            }
            catch (Exception e)
            {
                _log.Warn("update service instance failed", e);
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public virtual void Update(InstanceChange instanceChange)
        {
            try
            {
                string changeType = instanceChange.ChangeType;
                Instance instance = instanceChange.Instance;
                if (string.IsNullOrWhiteSpace(changeType) || instance == null)
                {
                    return;
                }

                ServiceContext currentContext = _services[instance.ServiceId.ToLower()];
                if (currentContext == null)
                {
                    return;
                }

                bool updated = false;
                if (InstanceChange.CHANGE_TYPE.DELETE.Equals(changeType))
                {
                    updated = currentContext.DeleteInstance(instance);
                }
                else if (InstanceChange.CHANGE_TYPE.NEW.Equals(changeType))
                {
                    updated = currentContext.AddInstance(instance);
                }
                else if (InstanceChange.CHANGE_TYPE.CHANGE.Equals(changeType))
                {
                    updated = currentContext.UpdateInstance(instance);
                }
                else
                {
                    _log.Info("unexpected changeType" + changeType);
                }

                if (updated)
                {
                    ServiceChangeEvent serviceChangeEvent = currentContext.newServiceChangeEvent(changeType);
                    foreach (ServiceChangeListener listener in currentContext.GetListeners())
                    {
                        NotifyServiceChange(listener, serviceChangeEvent);
                    }

                    _log.Info("Operation:" + changeType + "\nInstance: " + instance);

                }
                Metric(changeType, updated, instance);

            }
            catch (Exception e)
            {
                _log.Warn("update service instance failed", e);
            }
        }

        private void NotifyServiceChange(ServiceChangeListener listener, ServiceChangeEvent serviceChangeEvent)
        {
            if (listener == null || serviceChangeEvent == null)
                return;

            try
            {
                _serviceChangeNotifies.Enqueue(() =>
                {
                    try
                    {
                        listener.OnChange(serviceChangeEvent);
                    }
                    catch (Exception e)
                    {
                        _log.Error("execute service change listener failed", e);
                    }
                });
            }
            catch (Exception ex)
            {
                _log.Error("enqueue service change notification failed.", ex);
            }
        }

        private void Metric(string changeType, bool updated, Service service)
        {
            if (string.IsNullOrWhiteSpace(changeType) || service == null)
            {
                return;
            }

            Instance fakeAllInstance = GetFakeAllInstance(service);
            if (fakeAllInstance == null)
                return;

            Metric(changeType, updated, fakeAllInstance);
        }

        private Instance GetFakeAllInstance(Service service)
        {
            List<Instance> instances = service.Instances;
            if (Conditions.IsNullOrEmpty(instances))
                return null;

            Instance sampleInstance = instances[0];
            if (sampleInstance == null)
                return null;

            return new Instance()
            {
                GroupId = sampleInstance.GroupId,
                HealthCheckUrl = sampleInstance.HealthCheckUrl,
                InstanceId = InstanceChanges.RELOAD_FAKE_INSTANCE_ID,
                MachineName = sampleInstance.MachineName,
                IP = sampleInstance.IP,
                Metadata = sampleInstance.Metadata,
                Port = sampleInstance.Port,
                Protocol = sampleInstance.Protocol,
                RegionId = sampleInstance.RegionId,
                ZoneId = sampleInstance.ZoneId,
                ServiceId = sampleInstance.ServiceId,
                Status = sampleInstance.Status,
                Url = sampleInstance.Url
            };
        }

        private void Metric(string changeType, bool updated, Instance instance)
        {

            if (string.IsNullOrWhiteSpace(changeType) || instance == null)
            {
                return;
            }

            string metricId = "service-discovery." + changeType + "." + updated + "." + instance;

            Dictionary<string, string> metadata = new Dictionary<string, string>();
            metadata["metric_name_distribution"] = _serviceDiscoveryMetricName;
            metadata["regionId"] = instance.RegionId;
            metadata["zoneId"] = instance.ZoneId;
            metadata["serviceId"] = instance.ServiceId;
            metadata["updated"] = updated ? "true" : "false";
            metadata["instanceId"] = instance.InstanceId;

            IEventMetric metric = _eventMetricManager.GetMetric(metricId, new MetricConfig(metadata));
            metric.AddEvent(changeType);
        }
    }
}
