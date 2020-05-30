using System;
using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Deploy;
using Vendr.Core.Api;
using Vendr.Core.Models;
using Vendr.Deploy.Artifacts;

namespace Vendr.Deploy.Connectors.ServiceConnectors
{
    [UdiDefinition(VendrConstants.UdiEntityType.TaxClass, UdiType.GuidUdi)]
    public class VendrTaxClassServiceConnector : VendrStoreEntityServiceConnectorBase<TaxClassArtifact, TaxClassReadOnly, TaxClass, TaxClassState>
    {
        public override int[] ProcessPasses => new [] 
        {
            2
        };

        public override string[] ValidOpenSelectors => new []
        {
            "this-and-descendants",
            "descendants"
        };

        public override string AllEntitiesRangeName => "ALL VENDR TAX CLASSES";

        public override string UdiEntityType => VendrConstants.UdiEntityType.TaxClass;

        public VendrTaxClassServiceConnector(IVendrApi vendrApi)
            : base(vendrApi)
        { }

        public override string GetEntityName(TaxClassReadOnly entity)
            => entity.Name;

        public override TaxClassReadOnly GetEntity(Guid id)
            => _vendrApi.GetTaxClass(id);

        public override IEnumerable<TaxClassReadOnly> GetEntities(Guid storeId)
            => _vendrApi.GetTaxClasses(storeId);

        public override TaxClassArtifact GetArtifact(GuidUdi udi, TaxClassReadOnly entity)
        {
            if (entity == null)
                return null;

            var storeUdi = new GuidUdi(VendrConstants.UdiEntityType.Store, entity.StoreId);

            var dependencies = new ArtifactDependencyCollection
            {
                new ArtifactDependency(storeUdi, true, ArtifactDependencyMode.Match)
            };

            var artifcat = new TaxClassArtifact(udi, storeUdi, dependencies)
            {
                Name = entity.Name,
                Alias = entity.Alias,
                DefaultTaxRate = entity.DefaultTaxRate,
                SortOrder = entity.SortOrder
            };

            // Country region tax rates
            var countryRegionTaxRateArtifacts = new List<CountryRegionTaxRateArtifact>();

            foreach (var countryRegionTaxRate in entity.CountryRegionTaxRates)
            {
                var crtrArtifact = new CountryRegionTaxRateArtifact
                {
                    TaxRate = countryRegionTaxRate.TaxRate
                };

                var countryDepUdi = new GuidUdi(VendrConstants.UdiEntityType.Country, countryRegionTaxRate.CountryId);
                var countryDep = new ArtifactDependency(countryDepUdi, false, ArtifactDependencyMode.Exist);
                dependencies.Add(countryDep);

                crtrArtifact.CountryId = countryDepUdi;

                if (countryRegionTaxRate.RegionId.HasValue)
                {
                    var regionDepUdi = new GuidUdi(VendrConstants.UdiEntityType.Country, countryRegionTaxRate.CountryId);
                    var regionDep = new ArtifactDependency(regionDepUdi, false, ArtifactDependencyMode.Exist);
                    dependencies.Add(regionDep);

                    crtrArtifact.RegionId = regionDepUdi;
                }

                countryRegionTaxRateArtifacts.Add(crtrArtifact);
            }

            artifcat.CountryRegionTaxRates = countryRegionTaxRateArtifacts;

            return artifcat;
        }

        public override void Process(ArtifactDeployState<TaxClassArtifact, TaxClassReadOnly> state, IDeployContext context, int pass)
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

        private void Pass2(ArtifactDeployState<TaxClassArtifact, TaxClassReadOnly> state, IDeployContext context)
        {
            using (var uow = _vendrApi.Uow.Create())
            {
                var artifact = state.Artifact;
                var entity = state.Entity?.AsWritable(uow) ?? TaxClass.Create(uow, artifact.Udi.Guid, artifact.StoreId.Guid, artifact.Alias, artifact.Name, artifact.DefaultTaxRate);

                entity.SetName(artifact.Name, artifact.Alias)
                    .SetDefaultTaxRate(artifact.DefaultTaxRate)
                    .SetSortOrder(artifact.SortOrder);

                // TODO: Clear all tax rates first?

                foreach (var crtr in artifact.CountryRegionTaxRates)
                {
                    crtr.CountryId.EnsureType(VendrConstants.UdiEntityType.Country);

                    if (crtr.RegionId == null)
                    {
                        entity.SetCountryTaxRate(crtr.CountryId.Guid, crtr.TaxRate);
                    }
                    else
                    {
                        crtr.RegionId.EnsureType(VendrConstants.UdiEntityType.Region);

                        entity.SetRegionTaxRate(crtr.CountryId.Guid, crtr.RegionId.Guid, crtr.TaxRate);
                    }
                }

                _vendrApi.SaveTaxClass(entity);

                uow.Complete();
            }
        }
    }
}
