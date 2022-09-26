using Vendr.Core.Models;
using Umbraco.Cms.Core;

namespace Vendr.Deploy
{
    internal static class VendrUdiGetterExtensions
    {
        public static GuidUdi GetUdi(this EntityBase entity)
        {
            if (entity is StoreReadOnly store)
                return store.GetUdi();

            if (entity is CountryReadOnly country)
                return country.GetUdi();

            if (entity is RegionReadOnly region)
                return region.GetUdi();

            if (entity is OrderStatusReadOnly orderStatus)
                return orderStatus.GetUdi();

            if (entity is CurrencyReadOnly currency)
                return currency.GetUdi();

            if (entity is ShippingMethodReadOnly shippingMethod)
                return shippingMethod.GetUdi();

            if (entity is PaymentMethodReadOnly paymentMethod)
                return paymentMethod.GetUdi();

            if (entity is TaxClassReadOnly taxClass)
                return taxClass.GetUdi();

            if (entity is EmailTemplateReadOnly emailTemplate)
                return emailTemplate.GetUdi();

            if (entity is PrintTemplateReadOnly printTemplate)
                return printTemplate.GetUdi();

            if (entity is ExportTemplateReadOnly exportTemplate)
                return exportTemplate.GetUdi();

            if (entity is DiscountReadOnly discount)
                return discount.GetUdi();

            if (entity is GiftCardReadOnly giftCard)
                return giftCard.GetUdi();

            if (entity is ProductAttributeReadOnly productAtrtibtue)
                return productAtrtibtue.GetUdi();

            return null;
        }

        public static GuidUdi GetUdi(this StoreReadOnly entity)
            => new GuidUdi(VendrConstants.UdiEntityType.Store, entity.Id);

        public static GuidUdi GetUdi(this CountryReadOnly entity)
            => new GuidUdi(VendrConstants.UdiEntityType.Country, entity.Id);

        public static GuidUdi GetUdi(this RegionReadOnly entity)
            => new GuidUdi(VendrConstants.UdiEntityType.Region, entity.Id);

        public static GuidUdi GetUdi(this OrderStatusReadOnly entity)
            => new GuidUdi(VendrConstants.UdiEntityType.OrderStatus, entity.Id);

        public static GuidUdi GetUdi(this CurrencyReadOnly entity)
            => new GuidUdi(VendrConstants.UdiEntityType.Currency, entity.Id);

        public static GuidUdi GetUdi(this ShippingMethodReadOnly entity)
            => new GuidUdi(VendrConstants.UdiEntityType.ShippingMethod, entity.Id);

        public static GuidUdi GetUdi(this PaymentMethodReadOnly entity)
            => new GuidUdi(VendrConstants.UdiEntityType.PaymentMethod, entity.Id);

        public static GuidUdi GetUdi(this TaxClassReadOnly entity)
            => new GuidUdi(VendrConstants.UdiEntityType.TaxClass, entity.Id);

        public static GuidUdi GetUdi(this EmailTemplateReadOnly entity)
            => new GuidUdi(VendrConstants.UdiEntityType.EmailTemplate, entity.Id);

        public static GuidUdi GetUdi(this PrintTemplateReadOnly entity)
            => new GuidUdi(VendrConstants.UdiEntityType.PrintTemplate, entity.Id);

        public static GuidUdi GetUdi(this ExportTemplateReadOnly entity)
            => new GuidUdi(VendrConstants.UdiEntityType.ExportTemplate, entity.Id);

        public static GuidUdi GetUdi(this DiscountReadOnly entity)
            => new GuidUdi(VendrConstants.UdiEntityType.Discount, entity.Id);

        public static GuidUdi GetUdi(this GiftCardReadOnly entity)
            => new GuidUdi(VendrConstants.UdiEntityType.GiftCard, entity.Id);

        public static GuidUdi GetUdi(this ProductAttributeReadOnly entity)
            => new GuidUdi(VendrConstants.UdiEntityType.ProductAttribute, entity.Id);
    }
}
