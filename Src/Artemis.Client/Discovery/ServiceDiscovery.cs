using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Timers;

using Com.Ctrip.Soa.Artemis.Common.Discovery;
using Com.Ctrip.Soa.Caravan.Configuration;
using Com.Ctrip.Soa.Artemis.Common;
using Com.Ctrip.Soa.Artemis.Client.Common;
using Com.Ctrip.Soa.Artemis.Client.Utils;
using Com.Ctrip.Soa.Artemis.Client.WebSocketSharp;
using Com.Ctrip.Soa.Artemis.Common.Text;
using Com.Ctrip.Soa.Artemis.Common.Condition;
using WebSocketSharp;

using Com.Ctrip.Soa.Caravan.Logging;

namespace Com.Ctrip.Soa.Artemis.Client.Discovery
{
    /// <summary>
    /// Created by fang_j 2016/08/01.
    /// </summary>
    public class ServiceDiscovery
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(ServiceDiscovery));
        private readonly ServiceRepository _serviceRepository;
        private readonly ArtemisDiscoveryHttpClient _discoveryHttpClient;
        private readonly IProperty<long> _ttl;
        private long _lastUpdateTime = DateTimeUtils.CurrentTimeInMilliseconds;
        private readonly WebSocketSessionContext _sessionContext;
        private readonly DynamicTimer _poller;
        private readonly ConcurrentDictionary<String, DiscoveryConfig> _reloadFailedDiscoveryConfigs = new ConcurrentDictionary<String, DiscoveryConfig>();

        public ServiceDiscovery(ServiceRepository serviceRepository, ArtemisClientConfig config)
        {
            Preconditions.CheckArgument(serviceRepository != null, "ServiceRepository should not be null");
            _serviceRepository = serviceRepository;
            _discoveryHttpClient = new ArtemisDiscoveryHttpClient(config);
            _ttl = config.ConfigurationManager.GetProperty(config.Key("service-discovery.ttl"), 15 * 60 * 1000L, 60 * 1000, 24 * 60 * 60 * 1000);

            _sessionContext = new WebSocketSessionContext(config, webSocket =>
            {
                try
                {
                    SubScribe(webSocket);
                }
                catch (Exception e)
                {
                    _log.Warn("subscibe service change message failed", e);
                }
            }, (webSocket, message) =>
            {
                try
                {
                    OnInstanceChange(message.Data.FromJson<InstanceChange>());
                }
                catch (Exception e)
                {
                    _log.Warn("convert message failed", e);
                }
            });

            var key = config.Key("service-discovery.dynamic-scheduled-thread.run-interval");
            _poller = new DynamicTimer(config.ConfigurationManager.GetProperty(key, 60 * 1000, 60 * 1000, 24 * 60 * 60 * 1000),
                () =>
                {
                    try
                    {
                        Reload(ReloadDiscoveryConfigs);
                    }
                    catch (Exception e)
                    {
                        _log.Warn("reload services failed", e);
                    }
                });
        }

        public void RegisterDiscoveryConfig(DiscoveryConfig config)
        {
            SubScribe(_sessionContext.Value, config);
        }

        public void OnServiceChange(Service service)
        {
            _serviceRepository.Update(service);
        }

        public void OnInstanceChange(InstanceChange instanceChange)
        {
            string serviceId = instanceChange.Instance.ServiceId;
            if (InstanceChange.CHANGE_TYPE.RELOAD.Equals(instanceChange.ChangeType))
            {
                Reload(_serviceRepository.GetDiscoveryConfig(serviceId));
            }
            else
            {
                _serviceRepository.Update(instanceChange);
            }

        }

        public Service GetService(DiscoveryConfig config)
        {
            return _discoveryHttpClient.GetService(config);
        }

        public List<DiscoveryConfig> ReloadDiscoveryConfigs
        {
            get
            {
                if (Expired)
                {
                    return _serviceRepository.DiscoveryConfigs;
                }

                Dictionary<String, DiscoveryConfig> discoveryConfigs = new Dictionary<string, DiscoveryConfig>(_reloadFailedDiscoveryConfigs);
                List<DiscoveryConfig> configs = discoveryConfigs.Values.ToList();
                foreach (ServiceContext serviceContext in _serviceRepository.ServiceContexts)
                {
                    if (discoveryConfigs.ContainsKey(serviceContext.DiscoveryConfig.ServiceId) || serviceContext.IsAvailable())
                    {
                        continue;
                    }
                    configs.Add(serviceContext.DiscoveryConfig);
                }

                return configs;
            }

        }

        internal void Reload(DiscoveryConfig config)
        {
            Reload(new List<DiscoveryConfig>()
            {
                config
            });
        }

        internal void Reload(List<DiscoveryConfig> configs)
        {
            try
            {
                if (Conditions.IsNullOrEmpty(configs))
                    return;

                _log.Info("start reload services.");
                List<Service> services = _discoveryHttpClient.GetServices(configs);
                DiscoveryConfig config = new DiscoveryConfig();
                foreach (Service service in services)
                {
                    if (service == null)
                    {
                        continue;
                    }
                    string serviceId = service.ServiceId;
                    if (string.IsNullOrWhiteSpace(serviceId))
                    {
                        continue;
                    }
                    OnServiceChange(service);
                    _reloadFailedDiscoveryConfigs.TryRemove(serviceId.ToLower(), out config);
                }

                _lastUpdateTime = DateTimeUtils.CurrentTimeInMilliseconds;
                _log.Info("end reload services");
            }
            catch (Exception e)
            {
                foreach (DiscoveryConfig config in configs)
                {
                    if (config == null)
                    {
                        continue;
                    }
                    string serviceId = config.ServiceId;
                    if (string.IsNullOrWhiteSpace(serviceId))
                    {
                        continue;
                    }
                    _reloadFailedDiscoveryConfigs[serviceId.ToLower()] = config;
                }
                throw e;
            }
        }

        internal void SubScribe(WebSocket webSocket)
        {
            try
            {
                foreach (DiscoveryConfig discoveryConfig in _serviceRepository.DiscoveryConfigs)
                {
                    SubScribe(webSocket, discoveryConfig);
                }
            }
            catch (Exception e)
            {
                _log.Warn("subscibe services failed", e);
            }
        }

        internal void SubScribe(WebSocket webSocket, DiscoveryConfig discoveryConfig)
        {
            try
            {
                if (discoveryConfig == null)
                {
                    return;
                }

                if (webSocket == null)
                {
                    return;
                }

                webSocket.Send(discoveryConfig.ToJson());
            }
            catch (Exception e)
            {
                _log.Warn("subscibe service failed", e);
            }

        }

        internal bool Expired
        {
            get { return DateTimeUtils.CurrentTimeInMilliseconds - _lastUpdateTime >= _ttl.Value; }
        }
    }
}
