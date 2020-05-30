using Umbraco.Core;
using Vendr.Core.Models;

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

            if (entity is DiscountReadOnly discount)
                return discount.GetUdi();

            if (entity is GiftCardReadOnly giftCard)
                return giftCard.GetUdi();

            return null;
        }

        public static GuidUdi GetUdi(this StoreReadOnly entity)
            => new GuidUdi(Constants.UdiEntityType.Store, entity.Id);

        public static GuidUdi GetUdi(this CountryReadOnly entity)
            => new GuidUdi(Constants.UdiEntityType.Country, entity.Id);

        public static GuidUdi GetUdi(this RegionReadOnly entity)
            => new GuidUdi(Constants.UdiEntityType.Region, entity.Id);

        public static GuidUdi GetUdi(this OrderStatusReadOnly entity)
            => new GuidUdi(Constants.UdiEntityType.OrderStatus, entity.Id);

        public static GuidUdi GetUdi(this CurrencyReadOnly entity)
            => new GuidUdi(Constants.UdiEntityType.Currency, entity.Id);

        public static GuidUdi GetUdi(this ShippingMethodReadOnly entity)
            => new GuidUdi(Constants.UdiEntityType.ShippingMethod, entity.Id);

        public static GuidUdi GetUdi(this PaymentMethodReadOnly entity)
            => new GuidUdi(Constants.UdiEntityType.PaymentMethod, entity.Id);

        public static GuidUdi GetUdi(this TaxClassReadOnly entity)
            => new GuidUdi(Constants.UdiEntityType.TaxClass, entity.Id);

        public static GuidUdi GetUdi(this EmailTemplateReadOnly entity)
            => new GuidUdi(Constants.UdiEntityType.EmailTemplate, entity.Id);

        public static GuidUdi GetUdi(this DiscountReadOnly entity)
            => new GuidUdi(Constants.UdiEntityType.Discount, entity.Id);

        public static GuidUdi GetUdi(this GiftCardReadOnly entity)
            => new GuidUdi(Constants.UdiEntityType.GiftCard, entity.Id);
    }
}
