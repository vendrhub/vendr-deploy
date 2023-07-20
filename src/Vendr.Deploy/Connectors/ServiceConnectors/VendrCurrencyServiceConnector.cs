using System;
using System.Collections.Generic;
using System.Linq;
using Vendr.Core.Api;
using Vendr.Core.Models;
using Vendr.Deploy.Artifacts;
using Vendr.Deploy.Configuration;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;

namespace Vendr.Deploy.Connectors.ServiceConnectors
{
    [UdiDefinition(VendrConstants.UdiEntityType.Currency, UdiType.GuidUdi)]
    public class VendrCurrencyServiceConnector : VendrStoreEntityServiceConnectorBase<CurrencyArtifact, CurrencyReadOnly, Currency, CurrencyState>
    {
        public override int[] ProcessPasses => new[]
        {
            2,4
        };

        public override string[] ValidOpenSelectors => new[]
        {
            "this",
            "this-and-descendants",
            "descendants"
        };

        public override string AllEntitiesRangeName => "All Vendr Currencies";

        public override string UdiEntityType => VendrConstants.UdiEntityType.Currency;

        public VendrCurrencyServiceConnector(IVendrApi vendrApi, VendrDeploySettingsAccessor settingsAccessor)
            : base(vendrApi, settingsAccessor)
        { }

        public override string GetEntityName(CurrencyReadOnly entity)
            => entity.Name;

        public override CurrencyReadOnly GetEntity(Guid id)
            => _vendrApi.GetCurrency(id);

        public override IEnumerable<CurrencyReadOnly> GetEntities(Guid storeId)
            => _vendrApi.GetCurrencies(storeId);

        public override CurrencyArtifact GetArtifact(GuidUdi udi, CurrencyReadOnly entity)
        {
            if (entity == null)
                return null;

            var storeUdi = new GuidUdi(VendrConstants.UdiEntityType.Store, entity.StoreId);

            var dependencies = new ArtifactDependencyCollection
            {
                new VendrArtifactDependency(storeUdi)
            };

            var artifcat = new CurrencyArtifact(udi, storeUdi, dependencies)
            {
                Name = entity.Name,
                Code = entity.Code,
                CultureName = entity.CultureName,
                FormatTemplate = entity.FormatTemplate,
                SortOrder = entity.SortOrder
            };

            // Allowed countries
            if (entity.AllowedCountries.Count > 0)
            {
                var allowedCountryArtifacts = new List<AllowedCountryArtifact>();

                foreach (var allowedCountry in entity.AllowedCountries.OrderBy(x => x.CountryId))
                {
                    var countryDepUdi = new GuidUdi(VendrConstants.UdiEntityType.Country, allowedCountry.CountryId);
                    var countryDep = new VendrArtifactDependency(countryDepUdi);

                    dependencies.Add(countryDep);

                    allowedCountryArtifacts.Add(new AllowedCountryArtifact { CountryUdi = countryDepUdi });
                }

                artifcat.AllowedCountries = allowedCountryArtifacts;
            }

            return artifcat;
        }

        public override void Process(ArtifactDeployState<CurrencyArtifact, CurrencyReadOnly> state, IDeployContext context, int pass)
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

        private void Pass2(ArtifactDeployState<CurrencyArtifact, CurrencyReadOnly> state, IDeployContext context)
        {
            _vendrApi.Uow.Execute(uow =>
            {
                var artifact = state.Artifact;

                artifact.Udi.EnsureType(VendrConstants.UdiEntityType.Currency);
                artifact.StoreUdi.EnsureType(VendrConstants.UdiEntityType.Store);

                var entity = state.Entity?.AsWritable(uow) ?? Currency.Create(uow, artifact.Udi.Guid, artifact.StoreUdi.Guid, artifact.Code, artifact.Name, artifact.CultureName);

                entity.SetName(artifact.Name)
                    .SetCode(artifact.Code)
                    .SetCulture(artifact.CultureName)
                    .SetCustomFormatTemplate(artifact.FormatTemplate)
                    .SetSortOrder(artifact.SortOrder);

                _vendrApi.SaveCurrency(entity);

                state.Entity = entity;

                uow.Complete();
            });
        }

        private void Pass4(ArtifactDeployState<CurrencyArtifact, CurrencyReadOnly> state, IDeployContext context)
        {
            _vendrApi.Uow.Execute(uow =>
            {
                var artifact = state.Artifact;
                var entity = _vendrApi.GetCurrency(state.Entity.Id).AsWritable(uow);

                var allowedCountriesToRemove = entity.AllowedCountries
                    .Where(x => artifact.AllowedCountries == null || !artifact.AllowedCountries.Any(y => y.CountryUdi.Guid == x.CountryId))
                    .ToList();

                if (artifact.AllowedCountries != null)
                {
                    foreach (var ac in artifact.AllowedCountries)
                    {
                        ac.CountryUdi.EnsureType(VendrConstants.UdiEntityType.Country);

                        entity.AllowInCountry(ac.CountryUdi.Guid);
                    }
                }

                foreach (var ac in allowedCountriesToRemove)
                {
                    entity.DisallowInCountry(ac.CountryId);
                }

                _vendrApi.SaveCurrency(entity);

                uow.Complete();
            });
        }
    }
}
