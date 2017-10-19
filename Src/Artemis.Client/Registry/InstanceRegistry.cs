using Com.Ctrip.Soa.Artemis.Client.Common;
using Com.Ctrip.Soa.Artemis.Common;
using Com.Ctrip.Soa.Artemis.Common.Condition;
using System;
using Com.Ctrip.Soa.Artemis.Common.Configuration;
using Com.Ctrip.Soa.Artemis.Common.Text;
using Com.Ctrip.Soa.Artemis.Common.Registry;
using System.Collections.Generic;
using System.Timers;
using Com.Ctrip.Soa.Artemis.Client.WebSocketSharp;
using WebSocketSharp;
using Com.Ctrip.Soa.Caravan.Metric;
using Com.Ctrip.Soa.Artemis.Client.Atomic;
using Com.Ctrip.Soa.Caravan.Configuration;
using Com.Ctrip.Soa.Artemis.Client.Utils;
using Com.Ctrip.Soa.Caravan.Logging;

namespace Com.Ctrip.Soa.Artemis.Client.Registry
{
    /// <summary>
    /// Created by fang_j 2016/08/09.
    /// </summary>
    public class InstanceRegistry
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(InstanceRegistry));
        private readonly InstanceRepository _instanceRepository;
        private readonly IProperty<int> _ttl;
        private readonly IProperty<int> _interval;
        private long _lastHeartbeatTime = DateTimeUtils.CurrentTimeInMilliseconds;
        private long _heartbeatAcceptStartTime = DateTimeUtils.CurrentTimeInMilliseconds;
        private readonly WebSocketSessionContext _sessionContext;
        private readonly IAuditMetric _prepareHeartbeatLatency;
        private readonly IAuditMetric _sendHeartbeatLatency;
        private readonly IAuditMetric _acceptHeartbeatLatency;
        private readonly IEventMetric _heartbeatStatus;
        private readonly DynamicTimer _heartbeater;

        public InstanceRegistry(InstanceRepository instanceRepository, ArtemisClientConfig config)
        {
            Preconditions.CheckArgument(instanceRepository != null, "instance repository");
            Preconditions.CheckArgument(config != null, "config");
            _instanceRepository = instanceRepository;
            _ttl = config.ConfigurationManager.GetProperty(config.Key("instance-registry.instance-ttl"), 20 * 1000, 5 * 1000, 24 * 60 * 60 * 1000);
            _interval = config.ConfigurationManager.GetProperty(config.Key("instance-registry.heartbeat-interval"), 5 * 1000, 500, 5 * 60 * 1000);
            
            Action<WebSocket> onOpen = (webSocket) => {
            };
            Action<WebSocket, MessageEventArgs> onMessage = (webSocket, message) =>
            {
                AcceptHeartbeat(message);
            };
            _sessionContext = new WebSocketSessionContext(config, onOpen, onMessage);
            
            var heartbeatStatusMetricConfig = new MetricConfig(new Dictionary<string, string>()
            {
                {"metric_name_distribution", config.Key("heartbeat.event.distribution")}
            });
            _heartbeatStatus = config.EventMetricManager.GetMetric(config.Key("heartbeat.event"), heartbeatStatusMetricConfig);
            _prepareHeartbeatLatency = config.AuditMetricManager.GetMetric(config.Key("heartbeat.prepare-latency"), new MetricConfig(new Dictionary<string, string>()
            {
                {"metric_name_distribution", config.Key("heartbeat.prepare-latency.distribution")}
            }));
            _sendHeartbeatLatency = config.AuditMetricManager.GetMetric(config.Key("heartbeat.send-latency"), new MetricConfig(new Dictionary<string, string>()
            {
                {"metric_name_distribution", config.Key("heartbeat.send-latency.distribution")}
            }));
            _acceptHeartbeatLatency = config.AuditMetricManager.GetMetric(config.Key("heartbeat.accept-latency"), new MetricConfig(new Dictionary<string, string>()
            {
                {"metric_name_distribution", config.Key("heartbeat.accept-latency.distribution")}
            }));

            _heartbeater = new DynamicTimer(config.ConfigurationManager.GetProperty(config.Key("instances-registry.heartbeat-interval.dynamic-scheduled-thread.run-interval"), 1000, 500, 90 * 1000),
                () =>
                {
                    CheckHeartbeat();
                });
        }

        public virtual void Register(List<FailedInstance> failedInstances)
        {
            try
            {
                if (Conditions.IsNullOrEmpty(failedInstances))
                {
                    return;
                }
                List<Instance> instances = new List<Instance>();
                foreach (FailedInstance failedInstance in failedInstances)
                {
                    if (failedInstances == null)
                    {
                        continue;
                    }
                    if (ErrorCodes.ReregisterErrorCodes.Contains(failedInstance.ErrorCode))
                    {
                            instances.Add(failedInstance.Instance);
                    }
                }
                _instanceRepository.RegisterToRemote(instances);
            }
            catch (Exception e)
            {
                _log.Error("register failed instances failed", e);
            }
        }

        protected virtual void AcceptHeartbeat(MessageEventArgs message)
        {
            try
            {
                HeartbeatResponse response = message.Data.FromJson<HeartbeatResponse>();
                if (response.ResponseStatus == null)
                {
                    _heartbeatStatus.AddEvent("null");
                }
                else
                {
                    _heartbeatStatus.AddEvent(response.ResponseStatus.Status);
                }
                _acceptHeartbeatLatency.AddValue(DateTimeUtils.CurrentTimeInMilliseconds - _heartbeatAcceptStartTime);
                if (response.ResponseStatus.IsServiceDown())
                {
                    _sessionContext.Markdown();
                }
                if (response.ResponseStatus.IsFail())
                {
                    _log.Warn("heartbeat failed: " + response.ResponseStatus.Message);
                }
                else if (response.ResponseStatus.isPartialFail())
                {
                    _log.Info("heartbeat partial failed: " + response.ResponseStatus.Message);
                }
                Register(response.FailedInstances);
            }
            catch (Exception e)
            {
                _log.Warn("handle heartbeat message failed", e);
            }
        }

        protected virtual void SendHeartbeat()
        {
            try
            {
                if (_sessionContext.Value == null)
                {
                    return;
                }

                long prepareHeartbeatStartTime = DateTimeUtils.CurrentTimeInMilliseconds;
                string message = _instanceRepository.HeartbeatRequest;
                _prepareHeartbeatLatency.AddValue(DateTimeUtils.CurrentTimeInMilliseconds - prepareHeartbeatStartTime);
                if (message == null)
                {
                    _log.Info("heartbeat message is null");
                    _lastHeartbeatTime = DateTimeUtils.CurrentTimeInMilliseconds;
                    return;
                }

                long sendHeartbeatStartTime = DateTimeUtils.CurrentTimeInMilliseconds;
                _sessionContext.Value.Send(message);
                _sendHeartbeatLatency.AddValue(DateTimeUtils.CurrentTimeInMilliseconds - sendHeartbeatStartTime);
                _lastHeartbeatTime = DateTimeUtils.CurrentTimeInMilliseconds;
                _heartbeatAcceptStartTime = DateTimeUtils.CurrentTimeInMilliseconds;
                
            }
            catch (Exception e)
            {
                _log.Warn("send heartbeat failed", e);
            }
        }

        private void CheckHeartbeat()
        {
            long heartbeatInterval = DateTimeUtils.CurrentTimeInMilliseconds - _lastHeartbeatTime;
            if (heartbeatInterval >= _ttl.Value)
            {
                _log.Warn("heartbeat interval time is more than " + _ttl.Value);
                this._sessionContext.Markdown();
            }

            if (heartbeatInterval >= _interval.Value)
            {
                SendHeartbeat();
            }
        }
    }
}
