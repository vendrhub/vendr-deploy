using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Deploy;
using Umbraco.Deploy.Connectors.ServiceConnectors;
using Umbraco.Deploy.Exceptions;
using Vendr.Core.Api;
using Vendr.Core.Models;
using Vendr.Deploy.Artifacts;

namespace Vendr.Deploy.Connectors.ServiceConnectors
{
    public abstract class VendrStoreEntityServiceConnectorBase<TArtifact, TEntityReadOnly, TEntityWritable, TEntityState> : ServiceConnectorBase<TArtifact, GuidUdi, ArtifactDeployState<TArtifact, TEntityReadOnly>>
        where TArtifact : StoreEntityArtifactBase
        where TEntityReadOnly : StoreAggregateBase<TEntityReadOnly, TEntityWritable, TEntityState>
        where TEntityWritable : TEntityReadOnly
        where TEntityState : StoreAggregateStateBase
    {
        protected readonly IVendrApi _vendrApi;

        public abstract int[] ProcessPasses { get; }

        public abstract string[] ValidOpenSelectors { get; }

        public abstract string AllEntitiesRangeName { get; }

        public abstract string UdiEntityType { get; }

        public VendrStoreEntityServiceConnectorBase(IVendrApi vendrApi)
        {
            _vendrApi = vendrApi;
        }

        public abstract string GetEntityName(TEntityReadOnly entity);

        public abstract TEntityReadOnly GetEntity(Guid id);

        public abstract IEnumerable<TEntityReadOnly> GetEntities(Guid storeId);

        public abstract TArtifact GetArtifact(GuidUdi udi, TEntityReadOnly entity);

        public override TArtifact GetArtifact(object o)
        {
            var entity = o as TEntityReadOnly;
            if (entity == null)
                throw new InvalidEntityTypeException(string.Format("Unexpected entity type \"{0}\".", o.GetType().FullName));

            return GetArtifact(entity.GetUdi(), entity);
        }

        public override TArtifact GetArtifact(GuidUdi udi)
        {
            EnsureType(udi);

            return GetArtifact(udi, GetEntity(udi.Guid));
        }

        public override NamedUdiRange GetRange(GuidUdi udi, string selector)
        {
            EnsureType(udi);

            if (udi.IsRoot)
            {
                EnsureSelector(selector, ValidOpenSelectors);

                return new NamedUdiRange(udi, AllEntitiesRangeName, selector);
            }

            var entity = GetEntity(udi.Guid);
            if (entity == null)
                throw new ArgumentException("Could not find an entity with the specified identifier.", nameof(udi));

            return GetRange(entity, selector);
        }

        public override NamedUdiRange GetRange(string entityType, string sid, string selector)
        {
            if (sid == "-1")
            {
                EnsureSelector(selector, ValidOpenSelectors);

                return new NamedUdiRange(Udi.Create(UdiEntityType), AllEntitiesRangeName, selector);
            }

            if (!Guid.TryParse(sid, out Guid result))
                throw new ArgumentException("Invalid identifier.", nameof(sid));

            var entity = GetEntity(result);
            if (entity == null)
                throw new ArgumentException("Could not find an entity with the specified identifier.", nameof(sid));

            return GetRange(entity, selector);
        }

        private NamedUdiRange GetRange(TEntityReadOnly e, string selector)
        {
            return new NamedUdiRange(e.GetUdi(), GetEntityName(e), selector);
        }

        public override void Explode(UdiRange range, List<Udi> udis)
        {
            EnsureType(range.Udi);

            if (range.Udi.IsRoot)
            {
                EnsureSelector(range, ValidOpenSelectors);

                var stores = _vendrApi.GetStores();

                foreach (var store in stores)
                {
                    udis.AddRange(GetEntities(store.Id).Select(e => e.GetUdi()));
                }
            }
            else
            {
                var entity = GetEntity(((GuidUdi)range.Udi).Guid);
                if (entity == null)
                    return;

                EnsureSelector(range.Selector, new[] { "this" });

                udis.Add(entity.GetUdi());
            }
        }

        public override ArtifactDeployState<TArtifact, TEntityReadOnly> ProcessInit(TArtifact art, IDeployContext context)
        {
            EnsureType(art.Udi);

            var entity = GetEntity(art.Udi.Guid);

            return ArtifactDeployState.Create(art, entity, this, ProcessPasses[0]);
        }
    }
}
