using Com.Ctrip.Soa.Artemis.Common;

namespace Com.Ctrip.Soa.Artemis.Client
{
    /// <summary>
    /// Created by fang_j 2016/08/01.
    /// </summary>
    public interface RegistryClient
    {
        void Register(params Instance[] instances);
        void Unregister(params Instance[] instances);
    }
}