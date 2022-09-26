using System;
using System.Collections.Generic;
using Vendr.Core.Api;
using Vendr.Core.Models;
using Vendr.Deploy.Configuration;

using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Vendr.Deploy.Connectors.ValueConnectors
{
    public class VendrStoreEntityPickerValueConnector : IValueConnector
    {
        private readonly IDataTypeService _dataTypeService;
        private readonly IVendrApi _venderApi;
        private readonly VendrDeploySettingsAccessor _settingsAccessor;

        public IEnumerable<string> PropertyEditorAliases => new[] { "Vendr.StoreEntityPicker" };

        public VendrStoreEntityPickerValueConnector(IDataTypeService dataTypeService,
            IVendrApi venderApi, VendrDeploySettingsAccessor settingsAccessor)
        {
            _dataTypeService = dataTypeService;
            _venderApi = venderApi;
            _settingsAccessor = settingsAccessor;
        }

        public string ToArtifact(object value, IPropertyType propertyType, ICollection<ArtifactDependency> dependencies)
        {
            var svalue = value as string;

            if (string.IsNullOrWhiteSpace(svalue))
                return null;

            if (!Guid.TryParse(svalue, out var entityId))
                return null;

            var entityType = GetPropertyEntityType(propertyType);
            if (entityType == null)
                return null;

            var entity = GetEntity(entityType, entityId);
            if (entity == null)
                return null;

            var udi = new GuidUdi(entityType, entity.Id);

            dependencies.Add(new VendrArtifactDependency(udi, ArtifactDependencyMode.Exist));

            return udi.ToString();
        }

        public object FromArtifact(string value, IPropertyType propertyType, object currentValue)
        {
            if (string.IsNullOrWhiteSpace(value) || !UdiHelper.TryParseGuidUdi(value, out var udi))
                return null;

            var entity = GetEntity(udi.EntityType, udi.Guid);
            if (entity != null)
                return entity.Id.ToString();

            return null;
        }

        private string GetPropertyEntityType(IPropertyType propertyType)
        {
            var dataType = _dataTypeService.GetDataType(propertyType.DataTypeId);

            var cfg = dataType.ConfigurationAs<Dictionary<string, object>>();

            if (cfg.ContainsKey("entityType"))
            {
                var entityType = cfg["entityType"]?.ToString();

                switch (entityType)
                {
                    case "OrderStatus":
                        return VendrConstants.UdiEntityType.OrderStatus;
                    case "Country":
                        return VendrConstants.UdiEntityType.Country;
                    case "ShippingMethod":
                        return VendrConstants.UdiEntityType.ShippingMethod;
                    case "PaymentMethod":
                        return VendrConstants.UdiEntityType.PaymentMethod;
                    case "Currency":
                        return VendrConstants.UdiEntityType.Currency;
                    case "TaxClass":
                        return VendrConstants.UdiEntityType.TaxClass;
                    case "EmailTemplate":
                        return VendrConstants.UdiEntityType.EmailTemplate;
                    case "Discount": // Not sure if discounts should transfer as these are "user generated"
                        return VendrConstants.UdiEntityType.Discount;
                }
            }

            return null;
        }

        private EntityBase GetEntity(string entityType, Guid id)
        {
            switch (entityType)
            {
                case VendrConstants.UdiEntityType.OrderStatus:
                    return _venderApi.GetOrderStatus(id);
                case VendrConstants.UdiEntityType.Country:
                    return _venderApi.GetCountry(id);
                case VendrConstants.UdiEntityType.ShippingMethod:
                    return _venderApi.GetShippingMethod(id);
                case VendrConstants.UdiEntityType.PaymentMethod:
                    return _venderApi.GetPaymentMethod(id);
                case VendrConstants.UdiEntityType.Currency:
                    return _venderApi.GetCurrency(id);
                case VendrConstants.UdiEntityType.TaxClass:
                    return _venderApi.GetTaxClass(id);
                case VendrConstants.UdiEntityType.EmailTemplate:
                    return _venderApi.GetEmailTemplate(id);
                case VendrConstants.UdiEntityType.Discount:  // Not sure if discounts should transfer as these are "user generated"
                    return _venderApi.GetDiscount(id);
            }

            return null;
        }
    }
}
