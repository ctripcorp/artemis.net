using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Com.Ctrip.Soa.Artemis.Common.Discovery;
using Com.Ctrip.Soa.Artemis.Common.Condition;

namespace Com.Ctrip.Soa.Artemis.Common.Util
{
    public static class DiscoveryConfigs
    {
        public const string APP_ID = "appid";
        public const string SUB_ENV = "subenv";

        public static String getAppId(DiscoveryConfig config)
        {
            return get(config, APP_ID);
        }

        public static String getSubEnv(DiscoveryConfig config)
        {
            return get(config, SUB_ENV);
        }

        public static void setAppId(DiscoveryConfig config, String value)
        {
            set(config, APP_ID, value);
        }

        public static void setSubEnv(DiscoveryConfig config, String value)
        {
            set(config, SUB_ENV, value);
        }

        private static String get(DiscoveryConfig config, String key)
        {
            if (config == null)
            {
                return null;
            }

            if (config.DiscoveryData == null)
            {
                return null;
            }
            return config.DiscoveryData[key];
        }

        private static void set(DiscoveryConfig config, String key, String value)
        {
            Preconditions.NotNull(config, "config");
            Preconditions.NotNull(key, "key");
            Preconditions.NotNullOrWhiteSpace(value, "value");
            Dictionary<string, string> data = config.DiscoveryData;
            if (data == null)
            {
                data = new Dictionary<string, string>();
                config.DiscoveryData = data;
            }

            data[key] = value;
        }
    }
}
