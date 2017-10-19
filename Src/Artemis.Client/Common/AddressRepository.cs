using System.Collections.Generic;
using Com.Ctrip.Soa.Artemis.Client.Atomic;
using System;
using System.Threading.Tasks;
using System.Threading;
using System.Timers;
using System.Linq;
using Com.Ctrip.Soa.Artemis.Common.Configuration;
using Com.Ctrip.Soa.Artemis.Common.Cluster;
using Com.Ctrip.Soa.Artemis.Common.Condition;
using Com.Ctrip.Soa.Artemis.Common.Text;
using Com.Ctrip.Soa.Caravan.Configuration;
using Com.Ctrip.Soa.Caravan.Utility;
using Com.Ctrip.Soa.Caravan.Logging;

namespace Com.Ctrip.Soa.Artemis.Client.Common
{
    /// <summary>
    /// Created by fang_j 2016/08/02.
    /// </summary>
    public class AddressRepository
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(AddressRepository));
        private readonly string _path;
        private readonly GetServiceNodesRequest _request;
        private readonly IProperty _domainUrl;
        private readonly AtomicReference<List<string>> _availableServiceUrls = new AtomicReference<List<string>>(new List<string>());
        private readonly DynamicTimer _timer;

        public AddressRepository(string clientId, ArtemisClientManagerConfig managerConfig, string path)
        {
            Preconditions.CheckArgument(!string.IsNullOrWhiteSpace(clientId), "clientId");
            Preconditions.CheckArgument(managerConfig != null, "manager config");
            Preconditions.CheckArgument(!string.IsNullOrWhiteSpace(path), "path");

            _path = path.Trim('/');
            _request = new GetServiceNodesRequest()
            {
                RegionId = DeploymentConfig.RegionId,
                ZoneId = DeploymentConfig.ZoneId
            };

            _domainUrl = managerConfig.ConfigurationManager.GetProperty(clientId + ".service.domain.url", "");
            _log.Info(string.Format("Artemis service url initialized with {0}", _domainUrl.Value));
            _timer = new DynamicTimer(managerConfig.ConfigurationManager.GetProperty(clientId + ".address-repository.dynamic-scheduled-thread.run-interval",
                5 * 60 * 1000, 5 * 60 * 1000, 30 * 60 * 1000),
               () =>
               {
                   Refresh();
               });
            Refresh();
        }

        public string Address
        {
            get
            {
                List<string> urls = _availableServiceUrls.Value;
                if (Conditions.IsNullOrEmpty(urls))
                {
                    return _domainUrl.Value;
                }
                return urls[Threads.ThreadLocalRandom.Next(urls.Count)];
            }
        }

        private void Refresh()
        {
            try
            {
                _log.Info("start refresh service urls");
                string domainUrl = _domainUrl.Value;
                if (string.IsNullOrWhiteSpace(domainUrl))
                {
                    _log.Error("domain Url should not be null or empty for artemis client");
                    return;
                }
                List<string> urls = this.GetUrlsFromService(domainUrl);
                if (!Conditions.IsNullOrEmpty(urls))
                {
                    _availableServiceUrls.Value = urls;
                }
            }
            catch (Exception e)
            {
                _log.Warn("refesh service urls failed", e);
            }
            finally
            {
                _log.Info("end refresh service urls");
            }
        }

        private List<string> GetUrlsFromService(string url)
        {
            try
            {
                string requestUrl = url.TrimEnd('/') + "/" + _path;
                GetServiceNodesResponse response = requestUrl.PostJsonToUrl(_request)
                    .FromJson<GetServiceNodesResponse>();
                if (response.Nodes != null)
                {
                    return response.Nodes.Where(node => !string.IsNullOrWhiteSpace(node.Url))
                        .Select(node => node.Url).ToList();
                }
            }
            catch (Exception e)
            {
                _log.Error("reset address repository failed", e);
            }
            return null;
        }
    }
}