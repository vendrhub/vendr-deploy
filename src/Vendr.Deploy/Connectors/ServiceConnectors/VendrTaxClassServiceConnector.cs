using System;
using System.Collections.Generic;
using System.Linq;
using Vendr.Core.Api;
using Vendr.Core.Models;
using Vendr.Deploy.Artifacts;

#if NETFRAMEWORK
using Umbraco.Core;
using Umbraco.Core.Deploy;
#else
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
#endif

namespace Vendr.Deploy.Connectors.ServiceConnectors
{
    [UdiDefinition(VendrConstants.UdiEntityType.TaxClass, UdiType.GuidUdi)]
    public class VendrTaxClassServiceConnector : VendrStoreEntityServiceConnectorBase<TaxClassArtifact, TaxClassReadOnly, TaxClass, TaxClassState>
    {
        public override int[] ProcessPasses => new[]
        {
            2,4
        };

        public override string[] ValidOpenSelectors => new[]
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
                new VendrArtifactDependency(storeUdi)
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
                var countryDep = new VendrArtifactDependency(countryDepUdi);
                dependencies.Add(countryDep);

                crtrArtifact.CountryUdi = countryDepUdi;

                if (countryRegionTaxRate.RegionId.HasValue)
                {
                    var regionDepUdi = new GuidUdi(VendrConstants.UdiEntityType.Country, countryRegionTaxRate.CountryId);
                    var regionDep = new VendrArtifactDependency(regionDepUdi);
                    dependencies.Add(regionDep);

                    crtrArtifact.RegionUdi = regionDepUdi;
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
                case 4:
                    Pass4(state, context);
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

                artifact.Udi.EnsureType(VendrConstants.UdiEntityType.TaxClass);
                artifact.StoreUdi.EnsureType(VendrConstants.UdiEntityType.Store);

                var entity = state.Entity?.AsWritable(uow) ?? TaxClass.Create(uow, artifact.Udi.Guid, artifact.StoreUdi.Guid, artifact.Alias, artifact.Name, artifact.DefaultTaxRate);

                entity.SetName(artifact.Name, artifact.Alias)
                    .SetDefaultTaxRate(artifact.DefaultTaxRate)
                    .SetSortOrder(artifact.SortOrder);

                _vendrApi.SaveTaxClass(entity);

                state.Entity = entity;

                uow.Complete();
            }
        }

        private void Pass4(ArtifactDeployState<TaxClassArtifact, TaxClassReadOnly> state, IDeployContext context)
        {
            using (var uow = _vendrApi.Uow.Create())
            {
                var artifact = state.Artifact;
                var entity = state.Entity.AsWritable(uow);

                // Should probably validate the entity type here too, but really
                // given we are using guids, the likelyhood of a matching guid
                // being for a different entity type are pretty slim
                var countryRegionTaxRatesToRemove = entity.CountryRegionTaxRates
                    .Where(x => artifact.CountryRegionTaxRates == null || !artifact.CountryRegionTaxRates.Any(y => y.CountryUdi.Guid == x.CountryId && y.RegionUdi?.Guid == x.RegionId))
                    .ToList();

                if (artifact.CountryRegionTaxRates != null)
                {
                    foreach (var crtr in artifact.CountryRegionTaxRates)
                    {
                        crtr.CountryUdi.EnsureType(VendrConstants.UdiEntityType.Country);

                        if (crtr.RegionUdi == null)
                        {
                            entity.SetCountryTaxRate(crtr.CountryUdi.Guid, crtr.TaxRate);
                        }
                        else
                        {
                            crtr.RegionUdi.EnsureType(VendrConstants.UdiEntityType.Region);

                            entity.SetRegionTaxRate(crtr.CountryUdi.Guid, crtr.RegionUdi.Guid, crtr.TaxRate);
                        }
                    }
                }

                foreach (var crtr in countryRegionTaxRatesToRemove)
                {
                    if (crtr.RegionId == null)
                    {
                        entity.ClearCountryTaxRate(crtr.CountryId);
                    }
                    else
                    {
                        entity.ClearRegionTaxRate(crtr.CountryId, crtr.RegionId.Value);
                    }
                }

                _vendrApi.SaveTaxClass(entity);

                uow.Complete();
            }
        }
    }
}
