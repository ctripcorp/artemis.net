using System.Collections.Generic;

namespace Com.Ctrip.Soa.Artemis.Common.Registry
{
    public interface IHasInstance
    {
        List<Instance> Instances { get; }
    }
}