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
                    .SetSku(artifact.Sku)
                    .SetImage(artifact.ImageId)
                    .SetSettings(artifact.PaymentProviderSettings, SetBehavior.Merge)
                    .ToggleFeatures(artifact.CanFetchPaymentStatuses, artifact.CanCapturePayments, artifact.CanCancelPayments, artifact.CanRefundPayments)
                    .SetSortOrder(artifact.SortOrder);

                // TaxClass
                if (artifact.TaxClassId != null)
                {
                    artifact.TaxClassId.EnsureType(VendrConstants.UdiEntityType.TaxClass);
                    // TODO: Check the payment method exists?
                    entity.SetTaxClass(artifact.TaxClassId.Guid);
                }
                else
                {
                    entity.ClearTaxClass();
                }

                // Prices
                var pricesToRemove = entity.Prices
                    .Where(x => !artifact.Prices.Any(y => y.CountryId?.Guid == x.CountryId 
                        && y.RegionId?.Guid == x.RegionId
                        && y.CurrencyId.Guid == x.CurrencyId))
                    .ToList();

                foreach (var price in artifact.Prices)
                {
                    price.CurrencyId.EnsureType(VendrConstants.UdiEntityType.Currency);

                    if (price.CountryId == null && price.RegionId == null)
                    {
                        entity.SetDefaultPriceForCurrency(price.CurrencyId.Guid, price.Value);
                    }
                    else 
                    {
                        price.CountryId.EnsureType(VendrConstants.UdiEntityType.Country);

                        if (price.RegionId != null)
                        {
                            price.RegionId.EnsureType(VendrConstants.UdiEntityType.Region);

                            entity.SetRegionPriceForCurrency(price.CountryId.Guid, price.RegionId.Guid, price.CurrencyId.Guid, price.Value);
                        }
                        else
                        {
                            entity.SetCountryPriceForCurrency(price.CountryId.Guid, price.CurrencyId.Guid, price.Value);
                        }
                    }
                }

                foreach (var price in pricesToRemove)
                {
                    if (price.CountryId == null && price.RegionId == null)
                    {
                        entity.ClearDefaultPriceForCurrency(price.CurrencyId);
                    }
                    else if (price.CountryId != null && price.RegionId == null)
                    {
                        entity.ClearCountryPriceForCurrency(price.CountryId.Value, price.CurrencyId);
                    }
                    else
                    {
                        entity.ClearRegionPriceForCurrency(price.CountryId.Value, price.RegionId.Value, price.CurrencyId);
                    }
                }

                // AllowedCountryRegions
                var allowedCountryRegionsToRemove = entity.AllowedCountryRegions
                    .Where(x => !artifact.AllowedCountryRegions.Any(y => y.CountryId.Guid == x.CountryId
                        && y.RegionId?.Guid == x.RegionId))
                    .ToList();

                foreach (var acr in artifact.AllowedCountryRegions)
                {
                    acr.CountryId.EnsureType(VendrConstants.UdiEntityType.Country);

                    if (acr.RegionId != null)
                    {
                        acr.RegionId.EnsureType(VendrConstants.UdiEntityType.Region);

                        entity.AllowInRegion(acr.CountryId.Guid, acr.RegionId.Guid);
                    }
                    else
                    {
                        entity.AllowInCountry(acr.CountryId.Guid);
                    }
                }

                foreach (var acr in allowedCountryRegionsToRemove)
                {
                    if (acr.RegionId != null)
                    {
                        entity.DisallowInRegion(acr.CountryId, acr.RegionId.Value);
                    }
                    else
                    {
                        entity.DisallowInCountry(acr.CountryId);
                    }
                }

                _vendrApi.SavePaymentMethod(entity);

                uow.Complete();
            }
        }
    }
}
