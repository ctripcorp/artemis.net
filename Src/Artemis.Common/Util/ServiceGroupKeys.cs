using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Com.Ctrip.Soa.Artemis.Common.Condition;

namespace Com.Ctrip.Soa.Artemis.Common.Util
{
    public class ServiceGroupKeys
    {
        public static string Of(Instance instance)
        {
            Preconditions.NotNull(instance, "instance");
            String groupId = instance.GroupId;
            groupId = ServiceGroups.IsDefaultGroupId(groupId) ? ServiceGroups.DEFAULT_GROUP_ID : groupId;

            return Of(instance.ServiceId, instance.RegionId, instance.ZoneId, groupId, instance.InstanceId);
        }

        public static string Of(params string[] groupIds)
        {
            Preconditions.NotNullOrEmpty(groupIds, "groupIds");
            return string.Join("/", groupIds).ToLower();
        }
    }
}
