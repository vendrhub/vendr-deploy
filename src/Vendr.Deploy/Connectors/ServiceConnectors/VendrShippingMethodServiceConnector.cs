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
    [UdiDefinition(VendrConstants.UdiEntityType.ShippingMethod, UdiType.GuidUdi)]
    public class VendrShippingMethodServiceConnector : VendrStoreEntityServiceConnectorBase<ShippingMethodArtifact, ShippingMethodReadOnly, ShippingMethod, ShippingMethodState>
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

        public override string AllEntitiesRangeName => "ALL VENDR SHIPPING METHODS";

        public override string UdiEntityType => VendrConstants.UdiEntityType.ShippingMethod;

        public VendrShippingMethodServiceConnector(IVendrApi vendrApi, VendrDeploySettingsAccessor settingsAccessor)
            : base(vendrApi, settingsAccessor)
        { }

        public override string GetEntityName(ShippingMethodReadOnly entity)
            => entity.Name;

        public override ShippingMethodReadOnly GetEntity(Guid id)
            => _vendrApi.GetShippingMethod(id);

        public override IEnumerable<ShippingMethodReadOnly> GetEntities(Guid storeId)
            => _vendrApi.GetShippingMethods(storeId);

        public override ShippingMethodArtifact GetArtifact(GuidUdi udi, ShippingMethodReadOnly entity)
        {
            if (entity == null)
                return null;

            var storeUdi = new GuidUdi(VendrConstants.UdiEntityType.Store, entity.StoreId);

            var dependencies = new ArtifactDependencyCollection
            {
                new VendrArtifactDependency(storeUdi)
            };

            var artifact = new ShippingMethodArtifact(udi, storeUdi, dependencies)
            {
                Name = entity.Name,
                Alias = entity.Alias,
                Sku = entity.Sku,
                ImageId = entity.ImageId, // Could be a UDI?
                SortOrder = entity.SortOrder
            };

            // Tax class
            if (entity.TaxClassId != null)
            {
                var taxClassDepUdi = new GuidUdi(VendrConstants.UdiEntityType.TaxClass, entity.TaxClassId.Value);
                var taxClassDep = new VendrArtifactDependency(taxClassDepUdi);

                dependencies.Add(taxClassDep);

                artifact.TaxClassUdi = taxClassDepUdi;
            }

            // Service prices
            if (entity.Prices.Count > 0)
            {
                var servicesPrices = new List<ServicePriceArtifact>();

                foreach (var price in entity.Prices)
                {
                    var spArtifact = new ServicePriceArtifact { Value = price.Value };

                    // Currency
                    var currencyDepUdi = new GuidUdi(VendrConstants.UdiEntityType.Currency, price.CurrencyId);
                    var currencyDep = new VendrArtifactDependency(currencyDepUdi);

                    dependencies.Add(currencyDep);

                    spArtifact.CurrencyUdi = currencyDepUdi;

                    // Country
                    if (price.CountryId.HasValue)
                    {
                        var countryDepUdi = new GuidUdi(VendrConstants.UdiEntityType.Country, price.CountryId.Value);
                        var countryDep = new VendrArtifactDependency(countryDepUdi);

                        dependencies.Add(countryDep);

                        spArtifact.CountryUdi = countryDepUdi;
                    }

                    // Region
                    if (price.RegionId.HasValue)
                    {
                        var regionDepUdi = new GuidUdi(VendrConstants.UdiEntityType.Region, price.RegionId.Value);
                        var regionDep = new VendrArtifactDependency(regionDepUdi);

                        dependencies.Add(regionDep);

                        spArtifact.RegionUdi = regionDepUdi;
                    }

                    servicesPrices.Add(spArtifact);
                }

                artifact.Prices = servicesPrices;
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
                    var countryDep = new VendrArtifactDependency(countryDepUdi);

                    dependencies.Add(countryDep);

                    acrArtifact.CountryUdi = countryDepUdi;

                    // Region
                    if (acr.RegionId.HasValue)
                    {
                        var regionDepUdi = new GuidUdi(VendrConstants.UdiEntityType.Region, acr.RegionId.Value);
                        var regionDep = new VendrArtifactDependency(regionDepUdi);

                        dependencies.Add(regionDep);

                        acrArtifact.RegionUdi = regionDepUdi;
                    }

                    allowedCountryRegions.Add(acrArtifact);
                }

                artifact.AllowedCountryRegions = allowedCountryRegions;
            }

            return artifact;
        }

        public override void Process(ArtifactDeployState<ShippingMethodArtifact, ShippingMethodReadOnly> state, IDeployContext context, int pass)
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

        private void Pass2(ArtifactDeployState<ShippingMethodArtifact, ShippingMethodReadOnly> state, IDeployContext context)
        {
            _vendrApi.Uow.Execute(uow =>
            {
                var artifact = state.Artifact;

                artifact.Udi.EnsureType(VendrConstants.UdiEntityType.ShippingMethod);
                artifact.StoreUdi.EnsureType(VendrConstants.UdiEntityType.Store);

                var entity = state.Entity?.AsWritable(uow) ?? ShippingMethod.Create(uow, artifact.Udi.Guid, artifact.StoreUdi.Guid, artifact.Alias, artifact.Name);

                entity.SetName(artifact.Name, artifact.Alias)
                    .SetSku(artifact.Sku)
                    .SetImage(artifact.ImageId)
                    .SetSortOrder(artifact.SortOrder);

                _vendrApi.SaveShippingMethod(entity);

                state.Entity = entity;

                uow.Complete();
            });
        }

        private void Pass4(ArtifactDeployState<ShippingMethodArtifact, ShippingMethodReadOnly> state, IDeployContext context)
        {
            _vendrApi.Uow.Execute(uow =>
            {
                var artifact = state.Artifact;
                var entity = state.Entity.AsWritable(uow);

                // TaxClass
                if (artifact.TaxClassUdi != null)
                {
                    artifact.TaxClassUdi.EnsureType(VendrConstants.UdiEntityType.TaxClass);
                    // TODO: Check the payment method exists?
                    entity.SetTaxClass(artifact.TaxClassUdi.Guid);
                }
                else
                {
                    entity.ClearTaxClass();
                }

                // Prices
                var pricesToRemove = entity.Prices
                    .Where(x => artifact.Prices == null || !artifact.Prices.Any(y => y.CountryUdi?.Guid == x.CountryId
                        && y.RegionUdi?.Guid == x.RegionId
                        && y.CurrencyUdi.Guid == x.CurrencyId))
                    .ToList();

                if (artifact.Prices != null)
                {
                    foreach (var price in artifact.Prices)
                    {
                        price.CurrencyUdi.EnsureType(VendrConstants.UdiEntityType.Currency);

                        if (price.CountryUdi == null && price.RegionUdi == null)
                        {
                            entity.SetDefaultPriceForCurrency(price.CurrencyUdi.Guid, price.Value);
                        }
                        else
                        {
                            price.CountryUdi.EnsureType(VendrConstants.UdiEntityType.Country);

                            if (price.RegionUdi != null)
                            {
                                price.RegionUdi.EnsureType(VendrConstants.UdiEntityType.Region);

                                entity.SetRegionPriceForCurrency(price.CountryUdi.Guid, price.RegionUdi.Guid, price.CurrencyUdi.Guid, price.Value);
                            }
                            else
                            {
                                entity.SetCountryPriceForCurrency(price.CountryUdi.Guid, price.CurrencyUdi.Guid, price.Value);
                            }
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
                    .Where(x => artifact.AllowedCountryRegions == null || !artifact.AllowedCountryRegions.Any(y => y.CountryUdi.Guid == x.CountryId
                        && y.RegionUdi?.Guid == x.RegionId))
                    .ToList();

                if (artifact.AllowedCountryRegions != null)
                {
                    foreach (var acr in artifact.AllowedCountryRegions)
                    {
                        acr.CountryUdi.EnsureType(VendrConstants.UdiEntityType.Country);

                        if (acr.RegionUdi != null)
                        {
                            acr.RegionUdi.EnsureType(VendrConstants.UdiEntityType.Region);

                            entity.AllowInRegion(acr.CountryUdi.Guid, acr.RegionUdi.Guid);
                        }
                        else
                        {
                            entity.AllowInCountry(acr.CountryUdi.Guid);
                        }
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

                _vendrApi.SaveShippingMethod(entity);

                uow.Complete();
            });
        }
    }
}
