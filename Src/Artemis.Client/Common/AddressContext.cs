using System;
using Com.Ctrip.Soa.Artemis.Client.Atomic;
using System.Text.RegularExpressions;
using Com.Ctrip.Soa.Artemis.Common.Configuration;
using Com.Ctrip.Soa.Artemis.Common.Condition;
using Com.Ctrip.Soa.Caravan.Configuration;
using Com.Ctrip.Soa.Caravan.Utility;
using Com.Ctrip.Soa.Caravan.Logging;
using Com.Ctrip.Soa.Artemis.Client.Utils;

namespace Com.Ctrip.Soa.Artemis.Client.Common
{
    /// <summary>
    /// Created by fang_j 2016/08/02.
    /// </summary>
    public class AddressContext
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(AddressContext));
        private static readonly Regex _httpSchema = new Regex("(^http://|^https://)", RegexOptions.IgnoreCase);
        private const string _wsPrefix = "ws://";

        private readonly AtomicBoolean _available = new AtomicBoolean(false);
        private readonly long _createTime = DateTimeUtils.CurrentTimeInMilliseconds;
        private readonly string _httpUrl;
        private readonly string _webSocketEndpoint;
        private readonly IProperty<int> _ttl;

        public AddressContext(string clientId, ArtemisClientManagerConfig config)
            : this(clientId, config, string.Empty, string.Empty)
        {
        }

        public AddressContext(string clientId, ArtemisClientManagerConfig managerConfig, string httpUrl, string wsEndpointSuffix)
        {
            Preconditions.CheckArgument(!string.IsNullOrWhiteSpace(clientId), "clientId");
            Preconditions.CheckArgument(managerConfig != null, "managerConfig");
            
            _ttl = managerConfig.ConfigurationManager.GetProperty(clientId + ".address.context-ttl", 5 * 60 * 1000, 5 * 60 * 1000, 30 * 60 * 1000);
            if (string.IsNullOrWhiteSpace(httpUrl))
            {
                _httpUrl = string.Empty;
                _webSocketEndpoint = string.Empty;
            }
            else
            {
                string url = httpUrl.Trim('/');
                _httpUrl = url;
                string wsEndpointPrefix = _httpSchema.Replace(url, _wsPrefix);
                if (!string.IsNullOrWhiteSpace(wsEndpointSuffix))
                {
                    _webSocketEndpoint = wsEndpointPrefix + "/" + wsEndpointSuffix.Trim('/');
                }

                _available.GetAndSet(true);
            }
        }

        public bool IsAvailable
        {
            get { return this._available; }
        }

        public bool IsExpired
        {
            get { return DateTimeUtils.CurrentTimeInMilliseconds >= _ttl.Value + _createTime; }
        }

        public string WebSocketEndpoint
        {
            get { return _webSocketEndpoint; }
        }

        public string HttpUrl
        {
            get { return _httpUrl; }
        }

        public string CustomHttpUrl(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return _httpUrl;
            }

            return _httpUrl + "/" + path.TrimStart('/');
        }

        public void MarkUnavailable()
        {
            if (_available.CompareAndSet(true, false))
            {
                _log.Info(_httpUrl + " mark unavailable");
            }
        }
    }
}
