using System;
using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Deploy;
using Vendr.Core.Api;
using Vendr.Core.Models;
using Vendr.Deploy.Artifacts;

namespace Vendr.Deploy.Connectors.ServiceConnectors
{
    [UdiDefinition(VendrConstants.UdiEntityType.Country, UdiType.GuidUdi)]
    public class VendrCountryServiceConnector : VendrStoreEntityServiceConnectorBase<CountryArtifact, CountryReadOnly, Country, CountryState>
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

        public override string AllEntitiesRangeName => "ALL VENDR COUNTRIES";

        public override string UdiEntityType => VendrConstants.UdiEntityType.Country;

        public VendrCountryServiceConnector(IVendrApi vendrApi)
            : base(vendrApi)
        { }

        public override string GetEntityName(CountryReadOnly entity)
            => entity.Name;

        public override CountryReadOnly GetEntity(Guid id)
            => _vendrApi.GetCountry(id);

        public override IEnumerable<CountryReadOnly> GetEntities(Guid storeId)
            => _vendrApi.GetCountries(storeId);

        public override CountryArtifact GetArtifact(GuidUdi udi, CountryReadOnly entity)
        {
            if (entity == null)
                return null;

            var storeUdi = new GuidUdi(VendrConstants.UdiEntityType.Store, entity.StoreId);

            var dependencies = new ArtifactDependencyCollection
            {
                new VendrArtifcateDependency(storeUdi)
            };

            var artifcat = new CountryArtifact(udi, storeUdi, dependencies)
            {
                Name = entity.Name,
                Code = entity.Code,
                SortOrder = entity.SortOrder
            };

            // Default currency
            if (entity.DefaultCurrencyId != null)
            {
                var currencyDepUdi = new GuidUdi(VendrConstants.UdiEntityType.Currency, entity.DefaultCurrencyId.Value);
                var currencyDep = new VendrArtifcateDependency(currencyDepUdi);
                
                dependencies.Add(currencyDep);

                artifcat.DefaultCurrencyId = currencyDepUdi;
            }

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

        public override void Process(ArtifactDeployState<CountryArtifact, CountryReadOnly> state, IDeployContext context, int pass)
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

        private void Pass2(ArtifactDeployState<CountryArtifact, CountryReadOnly> state, IDeployContext context)
        {
            using (var uow = _vendrApi.Uow.Create())
            {
                var artifact = state.Artifact;

                artifact.Udi.EnsureType(VendrConstants.UdiEntityType.Country);
                artifact.StoreId.EnsureType(VendrConstants.UdiEntityType.Store);

                var entity = state.Entity?.AsWritable(uow) ?? Country.Create(uow, artifact.Udi.Guid, artifact.StoreId.Guid, artifact.Code, artifact.Name);

                entity.SetName(artifact.Name)
                    .SetCode(artifact.Code)
                    .SetSortOrder(artifact.SortOrder);

                if (artifact.DefaultCurrencyId != null)
                {
                    artifact.DefaultCurrencyId.EnsureType(VendrConstants.UdiEntityType.Currency);
                    // TODO: Check the currency exists?
                    entity.SetDefaultCurrency(artifact.DefaultCurrencyId.Guid);
                }

                if (artifact.DefaultPaymentMethodId != null)
                {
                    artifact.DefaultPaymentMethodId.EnsureType(VendrConstants.UdiEntityType.PaymentMethod);
                    // TODO: Check the payment method exists?
                    entity.SetDefaultPaymentMethod(artifact.DefaultPaymentMethodId.Guid);
                }

                if (artifact.DefaultShippingMethodId != null)
                {
                    artifact.DefaultShippingMethodId.EnsureType(VendrConstants.UdiEntityType.ShippingMethod);
                    // TODO: Check the payment method exists?
                    entity.SetDefaultShippingMethod(artifact.DefaultShippingMethodId.Guid);
                }

                _vendrApi.SaveCountry(entity);

                uow.Complete();
            }
        }
    }
}
