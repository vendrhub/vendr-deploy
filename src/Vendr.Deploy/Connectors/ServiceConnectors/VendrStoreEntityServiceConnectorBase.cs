using System;
using System.Collections.Generic;
using Vendr.Core.Api;
using Vendr.Core.Models;
using Vendr.Deploy.Artifacts;

namespace Vendr.Deploy.Connectors.ServiceConnectors
{
    public abstract class VendrStoreEntityServiceConnectorBase<TArtifact, TEntityReadOnly, TEntityWritable, TEntityState> : VendrEntityServiceConnectorBase<TArtifact, TEntityReadOnly>
        where TArtifact : StoreEntityArtifactBase
        where TEntityReadOnly : StoreAggregateBase<TEntityReadOnly, TEntityWritable, TEntityState>
        where TEntityWritable : TEntityReadOnly
        where TEntityState : StoreAggregateStateBase
    {
        public VendrStoreEntityServiceConnectorBase(IVendrApi vendrApi)
            : base(vendrApi)
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
