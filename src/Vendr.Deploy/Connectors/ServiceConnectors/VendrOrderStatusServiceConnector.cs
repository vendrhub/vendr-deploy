using System;
using System.Collections.Generic;
using Vendr.Core.Api;
using Vendr.Core.Models;
using Vendr.Deploy.Artifacts;
using Vendr.Deploy.Configuration;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;

namespace Vendr.Deploy.Connectors.ServiceConnectors
{
    [UdiDefinition(VendrConstants.UdiEntityType.OrderStatus, UdiType.GuidUdi)]
    public class VendrOrderStatusServiceConnector : VendrStoreEntityServiceConnectorBase<OrderStatusArtifact, OrderStatusReadOnly, OrderStatus, OrderStatusState>
    {
        public override int[] ProcessPasses => new[]
        {
            2
        };

        public override string[] ValidOpenSelectors => new[]
        {
            "this",
            "this-and-descendants",
            "descendants"
        };

        public override string AllEntitiesRangeName => "All Vendr Order Statuses";

        public override string UdiEntityType => VendrConstants.UdiEntityType.OrderStatus;

        public VendrOrderStatusServiceConnector(IVendrApi vendrApi, VendrDeploySettingsAccessor settingsAccessor)
            : base(vendrApi, settingsAccessor)
        { }

        public override string GetEntityName(OrderStatusReadOnly entity)
            => entity.Name;

        public override OrderStatusReadOnly GetEntity(Guid id)
            => _vendrApi.GetOrderStatus(id);

        public override IEnumerable<OrderStatusReadOnly> GetEntities(Guid storeId)
            => _vendrApi.GetOrderStatuses(storeId);

        public override OrderStatusArtifact GetArtifact(GuidUdi udi, OrderStatusReadOnly entity)
        {
            if (entity == null)
                return null;

            var storeUdi = new GuidUdi(VendrConstants.UdiEntityType.Store, entity.StoreId);

            var dependencies = new ArtifactDependencyCollection
            {
                new VendrArtifactDependency(storeUdi)
            };

            return new OrderStatusArtifact(udi, storeUdi, dependencies)
            {
                Name = entity.Name,
                Alias = entity.Alias,
                Color = entity.Color,
                SortOrder = entity.SortOrder
            };
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
            _vendrApi.Uow.Execute(uow =>
            {
                var artifact = state.Artifact;

                artifact.Udi.EnsureType(VendrConstants.UdiEntityType.OrderStatus);
                artifact.StoreUdi.EnsureType(VendrConstants.UdiEntityType.Store);

                var entity = state.Entity?.AsWritable(uow) ?? OrderStatus.Create(uow, artifact.Udi.Guid, artifact.StoreUdi.Guid, artifact.Alias, artifact.Name);

                entity.SetName(artifact.Name, artifact.Alias)
                    .SetColor(artifact.Color)
                    .SetSortOrder(artifact.SortOrder);

                _vendrApi.SaveOrderStatus(entity);

                uow.Complete();
            });
        }
    }
}
