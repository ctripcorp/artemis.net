using System;
using System.Threading;
using Com.Ctrip.Soa.Artemis.Common.Configuration;
using Com.Ctrip.Soa.Artemis.Common;
using Com.Ctrip.Soa.Artemis.Common.Text;
using System.Collections.Generic;
using Com.Ctrip.Soa.Caravan.Metric;
using Com.Ctrip.Soa.Caravan.Configuration;
using Com.Ctrip.Soa.Caravan.Logging;

namespace Com.Ctrip.Soa.Artemis.Client.Common
{
    /// <summary>
    /// Created by fang_j 2016/08/02.
    /// </summary>
    public class ArtemisHttpClient
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(ArtemisHttpClient));
        private readonly AddressManager _addressManager;
        private readonly IProperty<int> _httpClientRetryTimes;
        private readonly IProperty<int> _retryInterval;
        private readonly String _distributionMetricName;
        protected readonly IEventMetricManager _eventMetricManager;

        public ArtemisHttpClient(ArtemisClientConfig config, string httpClientId)
        {
            _addressManager = config.AddressManager;
            _httpClientRetryTimes = config.ConfigurationManager.GetProperty(httpClientId + ".http-client.retry-times", 5, 1, 10);
            _retryInterval = config.ConfigurationManager.GetProperty(httpClientId + ".http-client.retry-interval", 100, 0, 1000);
            _distributionMetricName = config.Key("http-response.status-code");
            _eventMetricManager = config.EventMetricManager;
        }

        public T Request<T>(string path, object request) where T : IHasResponseStatus
        {
            int retryTime = _httpClientRetryTimes.Value;
            ResponseStatus responseStatus = null;
            for (int i = 0; i < retryTime; i++)
            {
                AddressContext context = null;
                try
                {
                    context = _addressManager.AddressContext;
                    string requestUrl = context.CustomHttpUrl(path);
                    T response = requestUrl.PostJsonToUrl(request).FromJson<T>();
                    if (response == null || response.ResponseStatus == null)
                        throw new Exception("Got null response or null response status.");

                    responseStatus = response.ResponseStatus;
                    bool isServiceDown = responseStatus.IsServiceDown();
                    bool isRerunnable = responseStatus.IsRerunnable();
                    if (!(isServiceDown || isRerunnable))
                        return response;

                    if (isServiceDown)
                        context.MarkUnavailable();

                    _log.Info("get response failed, but can be retried. at turn: " + (i + 1) + ". responseStatus: " + responseStatus);
                }
                catch (Exception e)
                {
                    if (context != null)
                        context.MarkUnavailable();

                    if (i < retryTime - 1)
                    {
                        _log.Info("get response failed in this turn: " + (i + 1), e);
                    }
                    else
                    {
                        _log.Error("get response failed at turn: " + retryTime, e);
                        throw;
                    }
                }

                Thread.Sleep(_retryInterval.Value);
            }

            throw new Exception("Got failed response: " + responseStatus);
        }

        protected void LogEvent(string service, string operation)
        {
            LogEvent(null, service, operation);
        }

        protected void LogEvent(ResponseStatus status, string service, string operation)
        {
            string metricId = _distributionMetricName + "|" + service + "|" + operation;
            Dictionary<string, string> metadata = new Dictionary<string, string>(){
                {"metric_name_distribution", _distributionMetricName},
                {"service", service},
                {"operation", operation}
            };
            IEventMetric metric = _eventMetricManager.GetMetric(metricId, new MetricConfig(metadata));
            if (status == null)
            {
                metric.AddEvent("null");
            }
            else
            {
                metric.AddEvent(status.ErrorCode);
            }
        }
    }
}
