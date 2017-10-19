using System;
using System.Threading;
using Com.Ctrip.Soa.Artemis.Client.Atomic;
using System.Threading.Tasks;
using Com.Ctrip.Soa.Artemis.Common.Configuration;
using Com.Ctrip.Soa.Artemis.Common;
using Com.Ctrip.Soa.Artemis.Common.Condition;
using Com.Ctrip.Soa.Caravan.Logging;

namespace Com.Ctrip.Soa.Artemis.Client.Common
{
    /// <summary>
    /// Created by fang_j 2016/08/01.
    /// </summary>
    public class AddressManager
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(AddressManager));
        private readonly string _clientId;
        private readonly ArtemisClientManagerConfig _managerConfig;
        private readonly AtomicReference<AddressContext> _addressContext = new AtomicReference<AddressContext>();
        private readonly Func<AddressContext> _newAddressContext;

        private AddressManager(string clientId, ArtemisClientManagerConfig managerConfig, Func<AddressContext> newAddressContext)
        {
            Preconditions.CheckArgument(!string.IsNullOrWhiteSpace(clientId), "clientId");
            Preconditions.CheckArgument(managerConfig != null, "manager config");
            Preconditions.CheckArgument(newAddressContext != null, "newAddressContext");
            _clientId = clientId;
            _managerConfig = managerConfig;
            _newAddressContext = newAddressContext;
            _addressContext.GetAndSet(_newAddressContext());
        }

        public AddressContext AddressContext
        {
            get
            {
                AddressContext context = this._addressContext;
                if (!context.IsAvailable || context.IsExpired)
                {
                    context = _newAddressContext();
                    this._addressContext.Value = context;
                }
                return context;
            }
        }

        public static AddressManager NewDiscoveryAddressManager(string clientId, ArtemisClientManagerConfig managerConfig)
        {
            AddressRepository addressRepository = new AddressRepository(clientId, managerConfig, RestPaths.CLUSTER_UP_DISCOVERY_NODES_FULL_PATH);
            return new AddressManager(clientId, managerConfig,
                () => {
                    return new AddressContext(clientId, managerConfig, addressRepository.Address, WebSocketPaths.SERVICE_CHANGE_DESTINATION);
                });
        }

        public static AddressManager NewRegistryAddressManager(string clientId, ArtemisClientManagerConfig managerConfig)
        {
            AddressRepository addressRepository = new AddressRepository(clientId, managerConfig, RestPaths.CLUSTER_UP_REGISTRY_NODES_FULL_PATH);
            return new AddressManager(clientId, managerConfig,
               () =>
               {
                   return new AddressContext(clientId, managerConfig, addressRepository.Address, WebSocketPaths.HEARTBEAT_DESTINATION);
               });
        }
    }
}
