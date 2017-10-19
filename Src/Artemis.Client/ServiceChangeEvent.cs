using Com.Ctrip.Soa.Artemis.Common;

namespace Com.Ctrip.Soa.Artemis.Client
{
    /// <summary>
    /// Created by fang_j 2016/08/01.
    /// </summary>
    public interface ServiceChangeEvent
    {
        string ChangeType { get; }
        Service ChangedService { get; }
    }
}
