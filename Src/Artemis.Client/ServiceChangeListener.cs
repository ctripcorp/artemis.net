namespace Com.Ctrip.Soa.Artemis.Client
{
    /// <summary>
    /// Created by fang_j 2016/08/01.
    /// </summary>
    public interface ServiceChangeListener
    {
        void OnChange(ServiceChangeEvent serviceChangeEvent);
    }
}