namespace Com.Ctrip.Soa.Artemis.Common.Configuration
{
    public class DeploymentConfig
    {
        private static string _regionId;
        private static string _zoneId;
       
        public static void Init(string regionId, string zoneId)
        {
            _regionId = regionId;
            _zoneId = zoneId;
        }

        public static string ZoneId
        {
            get { return _zoneId; }
        }
        public static string RegionId
        {
            get { return _regionId; }
        }
    }
}
