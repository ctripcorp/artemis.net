using Com.Ctrip.Soa.Artemis.Client.Common;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Com.Ctrip.Soa.Artemis.Common;
using Com.Ctrip.Soa.Artemis.Common.Text;
using Com.Ctrip.Soa.Artemis.Client.Atomic;
using System;
using Com.Ctrip.Soa.Artemis.Common.Registry;
using Com.Ctrip.Soa.Caravan.Metric;
using Com.Ctrip.Soa.Artemis.Common.Condition;
using Com.Ctrip.Soa.Artemis.Client.Utils;
using Com.Ctrip.Soa.Caravan.Logging;

namespace Com.Ctrip.Soa.Artemis.Client.Registry
{
    /// <summary>
    /// Created by fang_j 2016/08/09.
    /// </summary>
    public class InstanceRepository
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(InstanceRepository));
        private readonly ArtemisRegistryHttpClient _client;
        private readonly HashSet<Instance> _instances = new HashSet<Instance>();
        private readonly List<IRegistryFilter> _filters;
        private readonly IAuditMetricManager _valueMetricManager;
        private readonly string _metricNameAudit;
        private readonly string _metricNameDistribution;

        public InstanceRepository(ArtemisClientConfig config)
        {
            Preconditions.CheckArgument(config != null, "config");
            _client = new ArtemisRegistryHttpClient(config);
            _filters = config.RegistryClientConfig.RegistryFilters;
            _valueMetricManager = config.AuditMetricManager;
            _metricNameAudit = config.Key("filter-instances.latency");
            _metricNameDistribution = config.Key("filter-instances.latency.distribution");
        }

        private List<Instance> FilterInstances(ICollection<Instance> instances)
        {
            if (instances == null || instances.Count == 0) {
                return new List<Instance>();
            }

            List<Instance> filterInstances = new List<Instance>(instances);

            if (_filters == null || _filters.Count == 0)
            {
                return filterInstances;
            }

            foreach (IRegistryFilter filter in _filters) {
                if (filter == null)
                {
                    continue;
                }
                long start = DateTimeUtils.CurrentTimeInMilliseconds;
                try
                {
                    filter.Filter(filterInstances);
                }
                catch (Exception e)
                {
                    _log.Warn("filter instances failed", e);
                }
                finally
                {
                    Metric(filter.RegistryFilterId, DateTimeUtils.CurrentTimeInMilliseconds - start);
                }
            }

            if (filterInstances == null)
            {
                return new List<Instance>();
            }
            else
            {
                return filterInstances;
            }
        }

        public List<Instance> AvailableInstances
        {
            get { return FilterInstances(_instances); }
        }

        public string HeartbeatRequest
        {
            get 
            {
                try
                {
                    List<Instance> instances = this.AvailableInstances;
                    if (instances.Count > 0)
                    {
                        HeartbeatRequest request = new HeartbeatRequest()
                        {
                            Instances = instances
                        };
                        return request.ToJson();
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception e)
                {
                    _log.Warn("get heartbeat message failed", e);
                    return null;
                }
            }
        }

        public void RegisterToRemote(List<Instance> instances)
        {
            if (Conditions.IsNullOrEmpty(instances))
            {
                return;
            }
            List<Instance> filterInstances = FilterInstances(instances);
            if (Conditions.IsNullOrEmpty(filterInstances))
            {
                _log.Info("get empty instances after RegistryFilter processed:" + String.Join(",", instances));
                return;
            }
            _client.Register(filterInstances);
        }

        public void Register(Instance[] instances)
        {
            if (instances == null || instances.Length == 0)
            {
                return;
            }
            _client.Unregister(new List<Instance>(instances));
            Update(instances, RegisterType.register);
        }

        public void Unregister(Instance[] instances)
        {
            if (instances == null || instances.Length == 0)
            {
                return;
            }
            
            _client.Unregister(new List<Instance>(instances));
            Update(instances, RegisterType.unregister);
        }

        public bool ContainsIntance(Instance instance)
        {
            return _instances.Contains(instance);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Update(Instance[] instances, RegisterType type)
        {
            try
            {
                if (instances == null || instances.Length == 0)
                {
                    return;
                }
                switch (type)
                {
                    case RegisterType.register:
                        foreach (Instance instance in instances)
                        {
                            _instances.Add(instance);
                        }
                        break;
                    case RegisterType.unregister:
                        foreach (Instance instance in instances)
                        {
                            _instances.Remove(instance);
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                _log.Warn("update instances failed", e);
            }
        }

        private void Metric(string filterId, long value)
        {
            if (string.IsNullOrWhiteSpace(filterId))
            {
                return;
            }
            string metricId = "filter-instances." + filterId;
       
            Dictionary<string, string> metadata = new Dictionary<string,string>();
            metadata["metric_name_distribution"] = _metricNameDistribution;
            metadata["metric_name_audit"] = _metricNameAudit;
            metadata["filter"] = filterId;

            IAuditMetric metric = _valueMetricManager.GetMetric(metricId, new MetricConfig(metadata));
            metric.AddValue(value);
        }
    }
}