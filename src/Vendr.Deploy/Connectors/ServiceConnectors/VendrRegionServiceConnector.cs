using System;
using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Deploy;
using Vendr.Core.Api;
using Vendr.Core.Models;
using Vendr.Deploy.Artifacts;

namespace Vendr.Deploy.Connectors.ServiceConnectors
{
    [UdiDefinition(VendrConstants.UdiEntityType.Region, UdiType.GuidUdi)]
    public class VendrRegionServiceConnector : VendrStoreEntityServiceConnectorBase<RegionArtifact, RegionReadOnly, Region, RegionState>
    {
        public override int[] ProcessPasses => new [] 
        {
            3,4
        };

        public override string[] ValidOpenSelectors => new []
        {
            "this-and-descendants",
            "descendants"
        };

        public override string AllEntitiesRangeName => "ALL VENDR REGIONS";

        public override string UdiEntityType => VendrConstants.UdiEntityType.Region;

        public VendrRegionServiceConnector(IVendrApi vendrApi)
            : base(vendrApi)
        { }

        public override string GetEntityName(RegionReadOnly entity)
            => entity.Name;

        public override RegionReadOnly GetEntity(Guid id)
            => _vendrApi.GetRegion(id);

        public override IEnumerable<RegionReadOnly> GetEntities(Guid storeId)
            => _vendrApi.GetRegions(storeId);

        public override RegionArtifact GetArtifact(GuidUdi udi, RegionReadOnly entity)
        {
            if (entity == null)
                return null;

            var storeUdi = new GuidUdi(VendrConstants.UdiEntityType.Store, entity.StoreId);
            var countryUdi = new GuidUdi(VendrConstants.UdiEntityType.Country, entity.CountryId);

            var dependencies = new ArtifactDependencyCollection
            {
                new VendrArtifcateDependency(storeUdi)
            };

            var artifcat = new RegionArtifact(udi, storeUdi, countryUdi, dependencies)
            {
                Name = entity.Name,
                Code = entity.Code,
                SortOrder = entity.SortOrder
            };

            // Default payment method
            if (entity.DefaultPaymentMethodId != null)
            {
                var pmDepUdi = new GuidUdi(VendrConstants.UdiEntityType.PaymentMethod, entity.DefaultPaymentMethodId.Value);
                var pmDep = new VendrArtifcateDependency(pmDepUdi);

                dependencies.Add(pmDep);

                artifcat.DefaultPaymentMethodId = pmDepUdi;
            }

            // Default shipping method
            if (entity.DefaultShippingMethodId != null)
            {
                var smDepUdi = new GuidUdi(VendrConstants.UdiEntityType.ShippingMethod, entity.DefaultShippingMethodId.Value);
                var smDep = new VendrArtifcateDependency(smDepUdi);

                dependencies.Add(smDep);

                artifcat.DefaultShippingMethodId = smDepUdi;
            }

            return artifcat;
        }

        public override void Process(ArtifactDeployState<RegionArtifact, RegionReadOnly> state, IDeployContext context, int pass)
        {
            state.NextPass = GetNextPass(ProcessPasses, pass);

            switch (pass)
            {
                case 3:
                    Pass3(state, context);
                    break;
                case 4:
                    Pass4(state, context);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(pass));
            }
        }

        private void Pass3(ArtifactDeployState<RegionArtifact, RegionReadOnly> state, IDeployContext context)
        {
            using (var uow = _vendrApi.Uow.Create())
            {
                var artifact = state.Artifact;

                artifact.Udi.EnsureType(VendrConstants.UdiEntityType.Region);
                artifact.StoreId.EnsureType(VendrConstants.UdiEntityType.Store);
                artifact.CountryId.EnsureType(VendrConstants.UdiEntityType.Country);

                var entity = state.Entity?.AsWritable(uow) ?? Region.Create(uow, artifact.Udi.Guid, artifact.StoreId.Guid, artifact.CountryId.Guid, artifact.Code, artifact.Name);

                entity.SetName(artifact.Name)
                    .SetCode(artifact.Code)
                    .SetSortOrder(artifact.SortOrder);

                _vendrApi.SaveRegion(entity);

                state.Entity = entity;

                uow.Complete();
            }
        }

        private void Pass4(ArtifactDeployState<RegionArtifact, RegionReadOnly> state, IDeployContext context)
        {
            using (var uow = _vendrApi.Uow.Create())
            {
                var artifact = state.Artifact;
                var entity = state.Entity.AsWritable(uow);

                if (artifact.DefaultPaymentMethodId != null)
                {
                    artifact.DefaultPaymentMethodId.EnsureType(VendrConstants.UdiEntityType.PaymentMethod);
                    // TODO: Check the payment method exists?
                }

                entity.SetDefaultPaymentMethod(artifact.DefaultPaymentMethodId?.Guid);

                if (artifact.DefaultShippingMethodId != null)
                {
                    artifact.DefaultShippingMethodId.EnsureType(VendrConstants.UdiEntityType.ShippingMethod);
                    // TODO: Check the payment method exists?
                }

                entity.SetDefaultShippingMethod(artifact.DefaultShippingMethodId?.Guid);

                _vendrApi.SaveRegion(entity);

                uow.Complete();
            }
        }
    }
}
