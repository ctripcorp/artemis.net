namespace Com.Ctrip.Soa.Artemis.Common
{
    public class InstanceChange
    {
        public class CHANGE_TYPE
        {
            public const string NEW = "new";
            public const string DELETE = "delete";
            public const string CHANGE = "change";
            public const string RELOAD = "reload";
        }

        public Instance Instance { get; set; }

        public string ChangeType { get; set; }

        public long ChangeTime { get; set; }
    }
}
