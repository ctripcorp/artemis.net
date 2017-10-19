using System.Collections.Generic;
using Com.Ctrip.Soa.Artemis.Common;

namespace Com.Ctrip.Soa.Artemis.Client
{
    public interface IRegistryFilter
    {
        string RegistryFilterId { get; }

        void Filter(List<Instance> instances);
    }
}
