namespace Com.Ctrip.Soa.Artemis.Common.Util
{
    public class ServiceGroups
    {
        public const string DEFAULT_GROUP_ID = "default";
        public const int MAX_WEIGHT_VALUE = 10000;
        public const int MIN_WEIGHT_VALUE = 0;
        public const int DEFAULT_WEIGHT_VALUE = 5;

        public static int FixWeight(int? weight)
        {
            if (weight == null || weight < MIN_WEIGHT_VALUE)
            {
                return DEFAULT_WEIGHT_VALUE;
            }

            if (weight > MAX_WEIGHT_VALUE)
            {
                return MAX_WEIGHT_VALUE;
            }

            return weight.Value;
        }

        public static bool isGroupCanaryInstance(string groupKey, Instance instance)
        {
            if (string.IsNullOrWhiteSpace(groupKey) || instance == null)
            {
                return false;
            }
            return RouteRules.DEFAULT_GROUP_KEY.Equals(groupKey) || ServiceGroupKeys.Of(instance).StartsWith(groupKey);
        }

        public static bool IsDefaultGroupId(string groupId)
        {
            if (string.IsNullOrWhiteSpace(groupId))
            {
                return true;
            }

            return DEFAULT_GROUP_ID.Equals(groupId.Trim());
        }
    }
}
