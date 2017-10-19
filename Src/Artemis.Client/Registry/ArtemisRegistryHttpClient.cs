using Com.Ctrip.Soa.Artemis.Client.Common;
using System.Collections.Generic;
using Com.Ctrip.Soa.Artemis.Common.Registry;
using Com.Ctrip.Soa.Artemis.Common;
using Com.Ctrip.Soa.Artemis.Common.Condition;
using Com.Ctrip.Soa.Artemis.Common.Configuration;
using System;
using Com.Ctrip.Soa.Artemis.Common.Text;
using Com.Ctrip.Soa.Caravan.Logging;

namespace Com.Ctrip.Soa.Artemis.Client.Registry
{
    public class ArtemisRegistryHttpClient : ArtemisHttpClient
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(ArtemisRegistryHttpClient));
        public ArtemisRegistryHttpClient(ArtemisClientConfig config)
            :base(config, config.Key("discovery"))
        { 
        }

        public void Register(List<Instance> instances)
        {
            try
            {
                Preconditions.CheckArgument(!Conditions.IsNullOrEmpty(instances), "instances");
                RegisterRequest request = new RegisterRequest() { Instances = instances };
                RegisterResponse response = this.Request<RegisterResponse>(RestPaths.REGISTRY_REGISTER_FULL_PATH, request);
                if (response.ResponseStatus.IsFail())
                {
                    _log.Error("register instances failed. Response:" + response.ToJson());
                }
                else if (response.ResponseStatus.isPartialFail())
                {
                    _log.Warn("register instances patial failed. Response:" + response.ToJson());
                }
                LogEvent(response.ResponseStatus, "registry", "register");
            }
            catch (Exception e)
            {
                _log.Warn("register instances failed", e);
                LogEvent("registry", "register");
            }
        }

        public void Unregister(List<Instance> instances)
        {
            try
            {
                Preconditions.CheckArgument(!Conditions.IsNullOrEmpty(instances), "instances");
                UnregisterRequest request = new UnregisterRequest() { Instances = instances };
                UnregisterRespnse response = this.Request<UnregisterRespnse>(RestPaths.REGISTRY_UNREGISTER_FULL_PATH, request);
                if (response.ResponseStatus.IsFail())
                {
                    _log.Error("unregister instances failed. Response:" + response.ToJson());
                }
                else if (response.ResponseStatus.isPartialFail())
                {
                    _log.Warn("unregister instances patial failed. Response:" + response.ToJson());
                }
                LogEvent(response.ResponseStatus, "registry", "unregister");
            }
            catch (Exception e)
            {
                _log.Warn("unregister instances failed", e);
                LogEvent("registry", "unregister");
            }
        }
    }
}
