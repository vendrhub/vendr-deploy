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

            dependencies.Add(new ArtifactDependency(udi, false, ArtifactDependencyMode.Exist));

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
                        return Constants.UdiEntityType.OrderStatus;
                    case "Country":
                        return Constants.UdiEntityType.Country;
                    case "ShippingMethod":
                        return Constants.UdiEntityType.ShippingMethod;
                    case "PaymentMethod":
                        return Constants.UdiEntityType.PaymentMethod;
                    case "Currency":
                        return Constants.UdiEntityType.Currency;
                    case "TaxClass":
                        return Constants.UdiEntityType.TaxClass;
                    case "EmailTemplate":
                        return Constants.UdiEntityType.EmailTemplate;
                    case "Discount": // Not sure if discounts should transfer as these are "user generated"
                        return Constants.UdiEntityType.Discount;
                }
            }

            return null;
        }

        private EntityBase GetEntity(string entityType, Guid id)
        {
            switch (entityType)
            {
                case Constants.UdiEntityType.OrderStatus:
                    return _venderApi.GetOrderStatus(id);
                case Constants.UdiEntityType.Country:
                    return _venderApi.GetCountry(id);
                case Constants.UdiEntityType.ShippingMethod:
                    return _venderApi.GetShippingMethod(id);
                case Constants.UdiEntityType.PaymentMethod:
                    return _venderApi.GetPaymentMethod(id);
                case Constants.UdiEntityType.Currency:
                    return _venderApi.GetCurrency(id);
                case Constants.UdiEntityType.TaxClass:
                    return _venderApi.GetTaxClass(id);
                case Constants.UdiEntityType.EmailTemplate:
                    return _venderApi.GetEmailTemplate(id);
                case Constants.UdiEntityType.Discount:  // Not sure if discounts should transfer as these are "user generated"
                    return _venderApi.GetDiscount(id);
            }

            return null;
        }
    }
}
