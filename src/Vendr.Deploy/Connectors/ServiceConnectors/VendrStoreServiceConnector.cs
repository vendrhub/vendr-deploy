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
    [UdiDefinition(Constants.UdiEntityType.Store, UdiType.GuidUdi)]
    public class VendrStoreServiceConnector : ServiceConnectorBase<StoreArtifact, GuidUdi, ArtifactDeployState<StoreArtifact, StoreReadOnly>>
    {
        private static readonly int[] ProcessPasses = new [] 
        {
            1,
            3
        };

        private static readonly string[] ValidOpenSelectors = new []
        {
            "this-and-descendants",
            "descendants"
        };

        private readonly IVendrApi _vendrApi;

        public VendrStoreServiceConnector(IVendrApi vendrApi)
        {
            _vendrApi = vendrApi;
        }

        public override StoreArtifact GetArtifact(object o)
        {
            var store = o as StoreReadOnly;
            if (store == null)
                throw new InvalidEntityTypeException(string.Format("Unexpected entity type \"{0}\".", (object)o.GetType().FullName));

            return GetArtifact(store.GetUdi(), store);
        }

        public override StoreArtifact GetArtifact(GuidUdi udi)
        {
            EnsureType(udi);

            return GetArtifact(udi, _vendrApi.GetStore(udi.Guid));
        }

        private StoreArtifact GetArtifact(GuidUdi udi, StoreReadOnly entity)
        {
            if (entity == null)
                return null;

            // TODO: Add the "defaults" as dependencies?
            // Need to know if Deploy enforces them existing prior to creating the store
            // entity as if that's the case, we can't have that, as store entities
            // require a store to exist prior their own creation

            var dependencies = new ArtifactDependencyCollection();

            var artifact = new StoreArtifact(udi, dependencies)
            {
                Name = entity.Name,
                Alias = entity.Alias
            };

            // Default order status
            if (entity.DefaultOrderStatusId.HasValue)
            {
                var depUdi = new GuidUdi(Constants.UdiEntityType.OrderStatus, entity.DefaultOrderStatusId.Value);
                var dep = new ArtifactDependency(depUdi, false, ArtifactDependencyMode.Exist);
                dependencies.Add(dep);
                artifact.DefaultOrderStatusId = depUdi;
            }

            return artifact;
        }

        public override NamedUdiRange GetRange(GuidUdi udi, string selector)
        {
            EnsureType(udi);

            if (udi.IsRoot)
            {
                EnsureSelector(selector, ValidOpenSelectors);

                return new NamedUdiRange(udi, "ALL VENDR STORES", selector);
            }

            var store = _vendrApi.GetStore(udi.Guid);
            if (store == null)
                throw new ArgumentException("Could not find an entity with the specified identifier.", nameof(udi));

            return GetRange(store, selector);
        }

        public override NamedUdiRange GetRange(string entityType, string sid, string selector)
        {
            if (sid == "-1")
            {
                EnsureSelector(selector, ValidOpenSelectors);
                return new NamedUdiRange(Udi.Create(Constants.UdiEntityType.Store), "ALL VENDR STORES", selector);
            }

            if (!Guid.TryParse(sid, out Guid result))
                throw new ArgumentException("Invalid identifier.", nameof(sid));

            
            var store = _vendrApi.GetStore(result);
            if (store == null)
                throw new ArgumentException("Could not find an entity with the specified identifier.", nameof(sid));

            return GetRange(store, selector);
        }

        private static NamedUdiRange GetRange(StoreReadOnly e, string selector)
        {
            return new NamedUdiRange(e.GetUdi(), e.Name, selector);
        }

        public override void Explode(UdiRange range, List<Udi> udis)
        {
            EnsureType(range.Udi);
            
            if (range.Udi.IsRoot)
            {
                EnsureSelector(range, ValidOpenSelectors);

                udis.AddRange(_vendrApi.GetStores().Select(e => e.GetUdi()));
            }
            else
            {
                var store = _vendrApi.GetStore(((GuidUdi)range.Udi).Guid);
                if (store == null)
                    return;

                EnsureSelector(range.Selector, new [] { "this" });

                udis.Add(store.GetUdi());
            }
        }

        public override ArtifactDeployState<StoreArtifact, StoreReadOnly> ProcessInit(StoreArtifact art, IDeployContext context)
        {
            EnsureType(art.Udi);

            var store = _vendrApi.GetStore(art.Udi.Guid);

            return ArtifactDeployState.Create(art, store, this, ProcessPasses[0]);
        }

        public override void Process(ArtifactDeployState<StoreArtifact, StoreReadOnly> state, IDeployContext context, int pass)
        {
            // TODO: NEED TO DO MULTI PASSES FOR INNER ENTITIES

            state.NextPass = GetNextPass(ProcessPasses, pass);

            switch (pass)
            {
                case 1:
                    Pass1(state, context);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(pass));
            }
        }

        private void Pass1(ArtifactDeployState<StoreArtifact, StoreReadOnly> state, IDeployContext context)
        {
            using (var uow = _vendrApi.Uow.Create())
            {
                var artifact = state.Artifact;
                var entity = state.Entity?.AsWritable(uow) ?? Store.Create(uow, artifact.Udi.Guid, artifact.Alias, artifact.Name);
                
                // Default Order Status
                // Not sure if this needs to occur in a later pass
                Guid? defaultOrderStatusId = null;
                if (artifact.DefaultOrderStatusId != null)
                {
                    artifact.DefaultOrderStatusId.EnsureType(Constants.UdiEntityType.OrderStatus);

                    defaultOrderStatusId = _vendrApi.GetOrderStatus(artifact.DefaultOrderStatusId.Guid)?.Id;
                }
                entity.SetDefaultOrderStatus(defaultOrderStatusId);

                _vendrApi.SaveStore(entity);

                uow.Complete();
            }
        }
    }
}
