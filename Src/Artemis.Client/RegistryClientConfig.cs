using System.Collections.Generic;
using Com.Ctrip.Soa.Artemis.Common;
using Com.Ctrip.Soa.Artemis.Common.Condition;

namespace Com.Ctrip.Soa.Artemis.Client
{
    public class RegistryClientConfig
    {
        public RegistryClientConfig()
        {
            RegistryFilters = new List<IRegistryFilter>();
        }

        public RegistryClientConfig(List<IRegistryFilter> registryFilters)
        {
            Preconditions.CheckArgument(registryFilters != null, "registry filters");
            RegistryFilters = registryFilters;
        }

        public List<IRegistryFilter> RegistryFilters { get; private set; }
    }
}