using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Deploy;
using Vendr.Core.Api;
using Vendr.Core.Models;
using Vendr.Deploy.Artifacts;

namespace Vendr.Deploy.Connectors.ServiceConnectors
{
    [UdiDefinition(VendrConstants.UdiEntityType.PaymentMethod, UdiType.GuidUdi)]
    public class VendrPaymentMethodServiceConnector : VendrStoreEntityServiceConnectorBase<PaymentMethodArtifact, PaymentMethodReadOnly, PaymentMethod, PaymentMethodState>
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

        public override string AllEntitiesRangeName => "ALL VENDR PAYMENT METHODS";

        public override string UdiEntityType => VendrConstants.UdiEntityType.PaymentMethod;

        public VendrPaymentMethodServiceConnector(IVendrApi vendrApi)
            : base(vendrApi)
        { }

        public override string GetEntityName(PaymentMethodReadOnly entity)
            => entity.Name;

        public override PaymentMethodReadOnly GetEntity(Guid id)
            => _vendrApi.GetPaymentMethod(id);

        public override IEnumerable<PaymentMethodReadOnly> GetEntities(Guid storeId)
            => _vendrApi.GetPaymentMethods(storeId);

        public override PaymentMethodArtifact GetArtifact(GuidUdi udi, PaymentMethodReadOnly entity)
        {
            if (entity == null)
                return null;

            var storeUdi = new GuidUdi(VendrConstants.UdiEntityType.Store, entity.StoreId);

            var dependencies = new ArtifactDependencyCollection
            {
                new VendrArtifcateDependency(storeUdi)
            };

            var artifcat = new PaymentMethodArtifact(udi, storeUdi, dependencies)
            {
                Name = entity.Name,
                Alias = entity.Alias,
                Sku = entity.Sku,
                ImageId = entity.ImageId, // Could be a UDI?
                PaymentProviderAlias = entity.PaymentProviderAlias,
                PaymentProviderSettings = entity.PaymentProviderSettings.ToDictionary(x => x.Key, x => x.Value), // Could contain UDIs?
                CanFetchPaymentStatuses = entity.CanFetchPaymentStatuses,
                CanCapturePayments = entity.CanCapturePayments,
                CanCancelPayments = entity.CanCancelPayments,
                CanRefundPayments = entity.CanRefundPayments,
                SortOrder = entity.SortOrder
            };

            // Tax class
            if (entity.TaxClassId != null)
            {
                var taxClassDepUdi = new GuidUdi(VendrConstants.UdiEntityType.TaxClass, entity.TaxClassId.Value);
                var taxClassDep = new VendrArtifcateDependency(taxClassDepUdi);
                
                dependencies.Add(taxClassDep);

                artifcat.TaxClassId = taxClassDepUdi;
            }

            // Service prices
            if (entity.Prices.Count > 0)
            {
                var servicesPrices = new List<ServicePriceArtifact>();

                foreach(var price in entity.Prices)
                {
                    var spArtifact = new ServicePriceArtifact { Value = price.Value };

                    // Currency
                    var currencyDepUdi = new GuidUdi(VendrConstants.UdiEntityType.Currency, price.CurrencyId);
                    var currencyDep = new VendrArtifcateDependency(currencyDepUdi);

                    dependencies.Add(currencyDep);

                    spArtifact.CurrencyId = currencyDepUdi;

                    // Country
                    if (price.CountryId.HasValue)
                    {
                        var countryDepUdi = new GuidUdi(VendrConstants.UdiEntityType.Country, price.CountryId.Value);
                        var countryDep = new VendrArtifcateDependency(countryDepUdi);

                        dependencies.Add(countryDep);

                        spArtifact.CountryId = countryDepUdi;
                    }

                    // Region
                    if (price.RegionId.HasValue)
                    {
                        var regionDepUdi = new GuidUdi(VendrConstants.UdiEntityType.Region, price.RegionId.Value);
                        var regionDep = new VendrArtifcateDependency(regionDepUdi);

                        dependencies.Add(regionDep);

                        spArtifact.RegionId = regionDepUdi;
                    }

                    servicesPrices.Add(spArtifact);
                }

                artifcat.Prices = servicesPrices;
            }

            // Allowed country regions
            if (entity.AllowedCountryRegions.Count > 0)
            {
                var allowedCountryRegions = new List<AllowedCountryRegionArtifact>();

                foreach (var acr in entity.AllowedCountryRegions)
                {
                    var acrArtifact = new AllowedCountryRegionArtifact();

                    // Country
                    var countryDepUdi = new GuidUdi(VendrConstants.UdiEntityType.Country, acr.CountryId);
                    var countryDep = new VendrArtifcateDependency(countryDepUdi);

                    dependencies.Add(countryDep);

                    acrArtifact.CountryId = countryDepUdi;

                    // Region
                    if (acr.RegionId.HasValue)
                    {
                        var regionDepUdi = new GuidUdi(VendrConstants.UdiEntityType.Region, acr.RegionId.Value);
                        var regionDep = new VendrArtifcateDependency(regionDepUdi);

                        dependencies.Add(regionDep);

                        acrArtifact.RegionId = regionDepUdi;
                    }

                    allowedCountryRegions.Add(acrArtifact);
                }

                artifcat.AllowedCountryRegions = allowedCountryRegions;
            }

            return artifcat;
        }

        public override void Process(ArtifactDeployState<PaymentMethodArtifact, PaymentMethodReadOnly> state, IDeployContext context, int pass)
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

        private void Pass2(ArtifactDeployState<PaymentMethodArtifact, PaymentMethodReadOnly> state, IDeployContext context)
        {
            using (var uow = _vendrApi.Uow.Create())
            {
                var artifact = state.Artifact;

                artifact.Udi.EnsureType(VendrConstants.UdiEntityType.PaymentMethod);
                artifact.StoreId.EnsureType(VendrConstants.UdiEntityType.Store);

                var entity = state.Entity?.AsWritable(uow) ?? PaymentMethod.Create(uow, artifact.Udi.Guid, artifact.StoreId.Guid, artifact.Alias, artifact.Name, artifact.PaymentProviderAlias);

                entity.SetName(artifact.Name, artifact.Alias)
                    .SetSortOrder(artifact.SortOrder);

                // TODO: Repopulate

                _vendrApi.SavePaymentMethod(entity);

                uow.Complete();
            }
        }
    }
}
