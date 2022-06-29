using System;
using System.Collections.Generic;
using Vendr.Core.Api;
using Vendr.Core.Models;
using Vendr.Deploy.Artifacts;
using Vendr.Deploy.Configuration;

namespace Vendr.Deploy.Connectors.ServiceConnectors
{
    public abstract class VendrStoreEntityServiceConnectorBase<TArtifact, TEntityReadOnly, TEntityWritable, TEntityState> : VendrEntityServiceConnectorBase<TArtifact, TEntityReadOnly>
        where TArtifact : StoreEntityArtifactBase
        where TEntityReadOnly : StoreAggregateBase<TEntityReadOnly, TEntityWritable, TEntityState>
        where TEntityWritable : TEntityReadOnly
        where TEntityState : StoreAggregateStateBase
    {
        public VendrStoreEntityServiceConnectorBase(IVendrApi vendrApi, VendrDeploySettingsAccessor settingsAccessor)
            : base(vendrApi, settingsAccessor)
        { }

        public override IEnumerable<TEntityReadOnly> GetEntities()
        {
            var stores = _vendrApi.GetStores();
            var storeEntities = new List<TEntityReadOnly>();

            foreach (var store in stores)
            {
                storeEntities.AddRange(GetEntities(store.Id));
            }

            return storeEntities;
        }

        public abstract IEnumerable<TEntityReadOnly> GetEntities(Guid storeId);
    }
}
