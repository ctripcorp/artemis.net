using Com.Ctrip.Soa.Artemis.Client.Common;
using Com.Ctrip.Soa.Artemis.Client.Atomic;
using System;
using Com.Ctrip.Soa.Artemis.Common.Configuration;
using Com.Ctrip.Soa.Artemis.Common.Condition;
using System.Timers;
using WebSocketSharp;
using Com.Ctrip.Soa.Caravan.Configuration;
using Com.Ctrip.Soa.Artemis.Client.Utils;

using Com.Ctrip.Soa.Caravan.Logging;

namespace Com.Ctrip.Soa.Artemis.Client.WebSocketSharp
{
    public class WebSocketSessionContext
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(WebSocketSessionContext));
        private readonly IProperty<int> _ttl;
        private readonly Action<WebSocket> _onOpen;
        private readonly Action<WebSocket, MessageEventArgs> _onMessage;
        private readonly AtomicReference<WebSocket> _session = new AtomicReference<WebSocket>();
        private long _lastUpdateTime = DateTimeUtils.CurrentTimeInMilliseconds;
        private readonly AtomicBoolean _isConnecting = new AtomicBoolean(false);
        private readonly DynamicTimer _healthChecker;
        private readonly AddressManager _addressManager;
        private readonly AtomicReference<AddressContext> _addressContext = new AtomicReference<AddressContext>();
        private readonly AtomicBoolean _isChecking = new AtomicBoolean(false);
        

        public WebSocketSessionContext(ArtemisClientConfig config, 
            Action<WebSocket> onOpen, Action<WebSocket, MessageEventArgs> onMessage)
        {
            Preconditions.CheckArgument(config != null, "config");
            Preconditions.CheckArgument(onOpen != null, "onOpen");
            Preconditions.CheckArgument(onMessage != null, "onMessage");
            _addressManager = config.AddressManager;
            _addressContext.Value = _addressManager.AddressContext;
            _ttl = config.ConfigurationManager.GetProperty(config.Key("websocket-session.ttl"), 5 * 60 * 1000, 5 * 60 * 1000, 30 * 60 * 1000);
            _onOpen = onOpen;
            _onMessage = onMessage;
            _healthChecker = new DynamicTimer(config.ConfigurationManager.GetProperty(config.Key("websocket-session.health-check.dynamic-scheduled-thread.run-interval"), 1000, 100, 10 * 60 * 1000),
                () =>
                {
                    CheckHealth();
                });
            CheckHealth();
        }

        public WebSocket Value
        {
            get { return _session.Value; }
        }

        public void Connect()
        {
            if (_isConnecting.CompareAndSet(false, true))
            {
                try
                {
                    AddressContext context = _addressManager.AddressContext;
                    if (!context.IsAvailable)
                    {
                        return;
                    }
                    
                    WebSocket currentWebSocket = new WebSocket(context.WebSocketEndpoint);
                    currentWebSocket.OnOpen += (o, e) => {
                        WebSocket oldWebSocket = _session.GetAndSet(currentWebSocket);
                        _lastUpdateTime = DateTimeUtils.CurrentTimeInMilliseconds;
                        _addressContext.Value = context;
                        if (_onOpen != null)
                        {
                            _onOpen(currentWebSocket);
                        }
                        Disconnect(oldWebSocket);
                    };

                    currentWebSocket.OnMessage += (o, e) => {
                        if (_onMessage != null)
                        {
                            _onMessage(currentWebSocket, e);
                        }
                    };

                    currentWebSocket.OnError += (o, e) =>
                    {
                        if (typeof(WebSocket).IsInstanceOfType(o))
                        {
                            Markdown();
                        }
                        _log.Info("WebSocket error:", e.Exception);
                    };

                    currentWebSocket.OnClose += (o, e) =>
                    {
                        _log.Info("WebSocket closed:" + e.Reason);
                        CheckHealth();
                    };
                    
                    currentWebSocket.Connect();
                }
                catch (Exception e)
                {
                    _log.Warn("connect websocket endpoint failed", e);
                }
                finally
                {
                    _isConnecting.Value = false;
                }
            }
        }

        public static void Disconnect(WebSocket webSocket)
        {
            try
            {
                if (webSocket != null)
                {
                    if (webSocket.ReadyState != WebSocketState.Closed && webSocket.ReadyState != WebSocketState.Closing)
                    {
                        webSocket.Close();
                    }
                }
            }
            catch (Exception e)
            {
                _log.Error("disconnect the WebSocketSession failed", e);
            }
        }

        internal bool IsAvailable(WebSocket webSocket)
        {
            return webSocket != null && webSocket.IsAlive;
        }

        internal bool IsExpired
        {
            get { return DateTimeUtils.CurrentTimeInMilliseconds >= _lastUpdateTime + _ttl.Value; }
        }


        private void CheckHealth()
        {
            if (_isChecking.CompareAndSet(false, true))
            {
                try
                {
                    bool available = _addressContext.Value.IsAvailable
                        && !IsExpired
                        && IsAvailable(_session.Value);

                    if (!available)
                    {
                        Connect();
                    }
                }
                catch (Exception e)
                {
                    _log.Error("WebSocket check health failed", e);
                }
                finally
                {
                    _isChecking.Value = false;
                }
            }
        }

        public void Markdown()
        {
            _addressContext.Value.MarkUnavailable();
            CheckHealth();
        }
    }
}
