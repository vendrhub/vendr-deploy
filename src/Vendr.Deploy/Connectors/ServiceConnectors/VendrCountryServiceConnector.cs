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
            2,4
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
                new VendrArtifcatDependency(storeUdi)
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
                var currencyDep = new VendrArtifcatDependency(currencyDepUdi);
                
                dependencies.Add(currencyDep);

                artifcat.DefaultCurrencyUdi = currencyDepUdi;
            }

            // Default payment method
            if (entity.DefaultPaymentMethodId != null)
            {
                var pmDepUdi = new GuidUdi(VendrConstants.UdiEntityType.PaymentMethod, entity.DefaultPaymentMethodId.Value);
                var pmDep = new VendrArtifcatDependency(pmDepUdi);

                dependencies.Add(pmDep);

                artifcat.DefaultPaymentMethodUdi = pmDepUdi;
            }

            // Default shipping method
            if (entity.DefaultShippingMethodId != null)
            {
                var smDepUdi = new GuidUdi(VendrConstants.UdiEntityType.ShippingMethod, entity.DefaultShippingMethodId.Value);
                var smDep = new VendrArtifcatDependency(smDepUdi);

                dependencies.Add(smDep);

                artifcat.DefaultShippingMethodUdi = smDepUdi;
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
                case 4:
                    Pass4(state, context);
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
                artifact.StoreUdi.EnsureType(VendrConstants.UdiEntityType.Store);

                var entity = state.Entity?.AsWritable(uow) ?? Country.Create(uow, artifact.Udi.Guid, artifact.StoreUdi.Guid, artifact.Code, artifact.Name);

                entity.SetName(artifact.Name)
                    .SetCode(artifact.Code)
                    .SetSortOrder(artifact.SortOrder);

                _vendrApi.SaveCountry(entity);

                state.Entity = entity;

                uow.Complete();
            }
        }

        private void Pass4(ArtifactDeployState<CountryArtifact, CountryReadOnly> state, IDeployContext context)
        {
            using (var uow = _vendrApi.Uow.Create())
            {
                var artifact = state.Artifact;
                var entity = state.Entity.AsWritable(uow);

                if (artifact.DefaultCurrencyUdi != null)
                {
                    artifact.DefaultCurrencyUdi.EnsureType(VendrConstants.UdiEntityType.Currency);
                    // TODO: Check the currency exists?
                }

                entity.SetDefaultCurrency(artifact.DefaultCurrencyUdi?.Guid);

                if (artifact.DefaultPaymentMethodUdi != null)
                {
                    artifact.DefaultPaymentMethodUdi.EnsureType(VendrConstants.UdiEntityType.PaymentMethod);
                    // TODO: Check the payment method exists?
                }

                entity.SetDefaultPaymentMethod(artifact.DefaultPaymentMethodUdi?.Guid);

                if (artifact.DefaultShippingMethodUdi != null)
                {
                    artifact.DefaultShippingMethodUdi.EnsureType(VendrConstants.UdiEntityType.ShippingMethod);
                    // TODO: Check the payment method exists?
                }

                entity.SetDefaultShippingMethod(artifact.DefaultShippingMethodUdi?.Guid);

                _vendrApi.SaveCountry(entity);

                uow.Complete();
            }
        }
    }
}
