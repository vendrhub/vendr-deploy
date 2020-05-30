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
    [UdiDefinition(Constants.UdiEntityType.OrderStatus, UdiType.GuidUdi)]
    public class VendrOrderStatusServiceConnector : ServiceConnectorBase<OrderStatusArtifact, GuidUdi, ArtifactDeployState<OrderStatusArtifact, OrderStatusReadOnly>>
    {
        private static readonly int[] ProcessPasses = new [] 
        {
            2
        };

        private static readonly string[] ValidOpenSelectors = new []
        {
            "this-and-descendants",
            "descendants"
        };

        private readonly IVendrApi _vendrApi;

        public VendrOrderStatusServiceConnector(IVendrApi vendrApi)
        {
            _vendrApi = vendrApi;
        }

        public override OrderStatusArtifact GetArtifact(object o)
        {
            var entity = o as OrderStatusReadOnly;
            if (entity == null)
                throw new InvalidEntityTypeException(string.Format("Unexpected entity type \"{0}\".", o.GetType().FullName));

            return GetArtifact(entity.GetUdi(), entity);
        }

        public override OrderStatusArtifact GetArtifact(GuidUdi udi)
        {
            EnsureType(udi);

            return GetArtifact(udi, _vendrApi.GetOrderStatus(udi.Guid));
        }

        private OrderStatusArtifact GetArtifact(GuidUdi udi, OrderStatusReadOnly entity)
        {
            if (entity == null)
                return null;

            var storeUdi = new GuidUdi(Constants.UdiEntityType.Store, entity.StoreId);

            var dependencies = new ArtifactDependencyCollection
            {
                new ArtifactDependency(storeUdi, true, ArtifactDependencyMode.Match)
            };

            return new OrderStatusArtifact(udi, storeUdi, dependencies)
            {
                Name = entity.Name,
                Alias = entity.Alias,
                Color = entity.Color,
                SortOrder = entity.SortOrder
            };
        }

        public override NamedUdiRange GetRange(GuidUdi udi, string selector)
        {
            EnsureType(udi);

            if (udi.IsRoot)
            {
                EnsureSelector(selector, ValidOpenSelectors);

                return new NamedUdiRange(udi, "ALL VENDR ORDER STATUSES", selector);
            }

            var entity = _vendrApi.GetOrderStatus(udi.Guid);
            if (entity == null)
                throw new ArgumentException("Could not find an entity with the specified identifier.", nameof(udi));

            return GetRange(entity, selector);
        }

        public override NamedUdiRange GetRange(string entityType, string sid, string selector)
        {
            if (sid == "-1") 
            {
                EnsureSelector(selector, ValidOpenSelectors);
                return new NamedUdiRange(Udi.Create(Constants.UdiEntityType.OrderStatus), "ALL VENDR ORDER STATUSES", selector);
            }

            if (!Guid.TryParse(sid, out Guid result))
                throw new ArgumentException("Invalid identifier.", nameof(sid));

            var entity = _vendrApi.GetOrderStatus(result);
            if (entity == null)
                throw new ArgumentException("Could not find an entity with the specified identifier.", nameof(sid));

            return GetRange(entity, selector);
        }

        private static NamedUdiRange GetRange(OrderStatusReadOnly e, string selector)
        {
            return new NamedUdiRange(e.GetUdi(), e.Name, selector);
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
                    udis.AddRange(_vendrApi.GetOrderStatuses(store.Id).Select(e => e.GetUdi()));
                }
            }
            else
            {
                var entity = _vendrApi.GetOrderStatus(((GuidUdi)range.Udi).Guid);
                if (entity == null)
                    return;

                EnsureSelector(range.Selector, new [] { "this" });

                udis.Add(entity.GetUdi());
            }
        }

        public override ArtifactDeployState<OrderStatusArtifact, OrderStatusReadOnly> ProcessInit(OrderStatusArtifact art, IDeployContext context)
        {
            EnsureType(art.Udi);

            var entity = _vendrApi.GetOrderStatus(art.Udi.Guid);

            return ArtifactDeployState.Create(art, entity, this, ProcessPasses[0]);
        }

        public override void Process(ArtifactDeployState<OrderStatusArtifact, OrderStatusReadOnly> state, IDeployContext context, int pass)
        {
            state.NextPass = GetNextPass(ProcessPasses, pass);

            switch (pass)
            {
                case 2:
                    Pass2(state, context);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(pass));
            }
        }

        private void Pass2(ArtifactDeployState<OrderStatusArtifact, OrderStatusReadOnly> state, IDeployContext context)
        {
            using (var uow = _vendrApi.Uow.Create())
            {
                var artifact = state.Artifact;
                var entity = state.Entity?.AsWritable(uow) ?? OrderStatus.Create(uow, artifact.Udi.Guid, artifact.StoreId.Guid, artifact.Alias, artifact.Name);

                entity.SetColor(artifact.Color)
                    .SetSortOrder(artifact.SortOrder);

                _vendrApi.SaveOrderStatus(entity);

                uow.Complete();
            }
        }
    }
}
