namespace Com.Ctrip.Soa.Artemis.Common.Registry
{
    public class FailedInstance
    {
        public Instance Instance { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
    }
}
