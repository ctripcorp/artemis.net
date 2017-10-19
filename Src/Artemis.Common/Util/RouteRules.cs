using System;
using System.Linq;
using System.Collections.Generic;
using Com.Ctrip.Soa.Artemis.Common.Condition;


namespace Com.Ctrip.Soa.Artemis.Common.Util
{
    public static class RouteRules
    {
        public const string CANARY_ROUTE_RULE = "canary-route-rule";
        public const string DEFAULT_ROUTE_RULE = "default-route-rule";
        public const string DEFAULT_GROUP_KEY = "default-group-key";
        public const string DEFAULT_GROUP_ID = "default";
        public const string DEFAULT_ROUTE_STRATEGY = RouteRule.STRATEGY.WEIGHTED_ROUND_ROBIN;

        public static List<RouteRule> NewRouteRules(this Service service)
        {
            List<RouteRule> routeRules = FilterRouteRules(service.RouteRules);
            Dictionary<string, Instance> groupKey2Instance = GenerateGroupKey2Instances(service.Instances);
            Dictionary<string, Instance> instanceId2Instance = GenerateInstanceId2Instance(service.Instances, service.LogicInstances);

            List<RouteRule> newRouteRules = new List<RouteRule>();
            HashSet<string> routeRuleIds = new HashSet<string>();
            foreach (RouteRule routeRule in routeRules)
            {
                List<ServiceGroup> nonemptyServiceGroups = new List<ServiceGroup>();
                foreach (ServiceGroup serviceGroup in routeRule.Groups)
                {
                    if (serviceGroup == null || string.IsNullOrWhiteSpace(serviceGroup.GroupKey))
                    {
                        continue;
                    }
                    string groupKey = serviceGroup.GroupKey.ToLower();
                    List<Instance> instances = GenerateGroupInstances(serviceGroup, instanceId2Instance, groupKey2Instance);
                    if (IsCanaryRouteRule(routeRule))
                    {
                        Dictionary<String, Instance> maps = new Dictionary<string, Instance>();
                        foreach (Instance instance in instances)
                        {
                            maps[instance.InstanceId] = instance;
                        }
                        foreach (Instance instance in FilterInstances(serviceGroup.Instances))
                        {
                            if (!maps.ContainsKey(instance.InstanceId))
                            {
                                instances.Add(instance);
                            }
                        }
                    }
                    
                    if (Conditions.IsNullOrEmpty(instances))
                    {
                        continue;
                    }

                    ServiceGroup nonemptyServiceGroup = new ServiceGroup()
                    {
                        GroupKey = serviceGroup.GroupKey,
                        Weight = ServiceGroups.FixWeight(serviceGroup.Weight),
                        Instances = instances,
                        Metadata = serviceGroup.Metadata == null ? null : new Dictionary<string, string>(serviceGroup.Metadata)
                    };

                    nonemptyServiceGroups.Add(nonemptyServiceGroup);
                }

                routeRuleIds.Add(routeRule.RouteId.ToLower());
                newRouteRules.Add(new RouteRule()
                {
                    RouteId = routeRule.RouteId,
                    Groups = nonemptyServiceGroups,
                    Strategy = routeRule.Strategy
                });
            }

            if (!routeRuleIds.Contains(DEFAULT_ROUTE_RULE))
            {
                newRouteRules.Add(newDefaultRouteRule(groupKey2Instance.Values.ToList()));
            }

            return newRouteRules;
        }

        public static List<RouteRule> FilterRouteRules(List<RouteRule> routeRules)
        {
            if (Conditions.IsNullOrEmpty(routeRules))
            {
                return new List<RouteRule>();
            }
            else
            {
                return routeRules.Where(routeRule =>
                routeRule != null
                && !string.IsNullOrWhiteSpace(routeRule.RouteId)
                && !Conditions.IsNullOrEmpty(routeRule.Groups)).ToList();
            }
        }

        public static List<Instance> FilterInstances(List<Instance> instances)
        {
            List<Instance> filterInstances = new List<Instance>();
            if (Conditions.IsNullOrEmpty(instances))
            {
                return filterInstances;
            }

            foreach (Instance instance in instances)
            {
                if (instance == null || string.IsNullOrWhiteSpace(instance.InstanceId))
                {
                    continue;
                }
                filterInstances.Add(instance);
            }
            return filterInstances;
        }

        public static List<Instance> GenerateGroupInstances(ServiceGroup serviceGroup, Dictionary<String, Instance> instanceId2Instance, Dictionary<String, Instance> groupKey2Instance)
        {
            string groupKey = serviceGroup.GroupKey == null ? null : serviceGroup.GroupKey.ToLower();
            Dictionary<string, Instance> instances = new Dictionary<string, Instance>();
            if (!Conditions.IsNullOrEmpty(serviceGroup.InstanceIds))
            {
                foreach (string instanceId in serviceGroup.InstanceIds)
                {
                    if (string.IsNullOrWhiteSpace(instanceId))
                    {
                        continue;
                    }
                    if (instanceId2Instance.ContainsKey(instanceId))
                    {
                        instances[instanceId] = instanceId2Instance[instanceId];
                    }
                }

            }

            if (!string.IsNullOrWhiteSpace(groupKey))
            {
                foreach (string key in groupKey2Instance.Keys)
                {
                    if (key.StartsWith(groupKey))
                    {
                        Instance currentInstance = groupKey2Instance[key];
                        instances[currentInstance.InstanceId] = currentInstance;
                    }
                }
            }

            return instances.Values.ToList();
        }

        public static Dictionary<string, Instance> GenerateGroupKey2Instances(List<Instance> instances)
        {
            Dictionary<string, Instance> groupInstances = new Dictionary<string, Instance>();

            foreach (Instance instance in FilterInstances(instances))
            {
                groupInstances[ServiceGroupKeys.Of(instance)] = instance;
            }

            return groupInstances;
        }

        public static Dictionary<String, Instance> GenerateInstanceId2Instance(List<Instance> instances, List<Instance> logicInstances)
        {
            Dictionary<String, Instance> groupInstances = new Dictionary<string, Instance>();

            foreach (Instance instance in FilterInstances(logicInstances))
            {
                groupInstances[instance.InstanceId] = instance;
            }

            foreach (Instance instance in FilterInstances(instances))
            {
                groupInstances[instance.InstanceId] = instance;
            }

            return groupInstances;
        }

        public static RouteRule newDefaultRouteRule(List<Instance> instances)
        {
            return new RouteRule()
            {
                RouteId = DEFAULT_ROUTE_RULE,
                Groups = new List<ServiceGroup>(){
                    new ServiceGroup(){
                        GroupKey = DEFAULT_GROUP_KEY,
                        Weight = ServiceGroups.FixWeight(null),
                        Instances = instances
                    }
                },
                Strategy = DEFAULT_ROUTE_STRATEGY
            };
        }

        public static bool IsDefaultRouteRule(RouteRule routeRule)
        {
            Preconditions.CheckArgument(routeRule != null, "route rule should not null");
            if (routeRule.RouteId == null)
            {
                return false;
            }
            return string.Equals(DEFAULT_ROUTE_RULE, routeRule.RouteId.Trim().ToLower());
        }

        public static bool IsCanaryRouteRule(RouteRule routeRule)
        {
            Preconditions.CheckArgument(routeRule != null, "route rule should not null");
            if (routeRule.RouteId == null)
            {
                return false;
            }
            return string.Equals(CANARY_ROUTE_RULE, routeRule.RouteId.Trim().ToLower());
        }
    }
}
