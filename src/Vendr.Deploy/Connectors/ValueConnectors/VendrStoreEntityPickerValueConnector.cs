using System;
using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Deploy;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Vendr.Core.Api;
using Vendr.Core.Models;

namespace Vendr.Deploy.Connectors.ValueConnectors
{
    public class VendrStoreEntityPickerValueConnector : IValueConnector
    {
        private readonly IDataTypeService _dataTypeService;
        private readonly IVendrApi _venderApi;

        public IEnumerable<string> PropertyEditorAliases => new[] { "Vendr.StoreEntityPicker" };

        public VendrStoreEntityPickerValueConnector(IDataTypeService dataTypeService,
            IVendrApi venderApi)
        {
            _dataTypeService = dataTypeService;
            _venderApi = venderApi;
        }

        public string ToArtifact(object value, PropertyType propertyType, ICollection<ArtifactDependency> dependencies)
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

            dependencies.Add(new VendrArtifcateDependency(udi));

            return udi.ToString();
        }

        public object FromArtifact(string value, PropertyType propertyType, object currentValue)
        {
            if (string.IsNullOrWhiteSpace(value) || !GuidUdi.TryParse(value, out var udi))
                return null;

            var entity = GetEntity(udi.EntityType, udi.Guid);
            if (entity != null)
                return entity.Id.ToString();

            return null;
        }

        private string GetPropertyEntityType(PropertyType propertyType)
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
