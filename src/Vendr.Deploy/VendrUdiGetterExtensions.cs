using Umbraco.Core;
using Vendr.Core.Models;

namespace Vendr.Deploy
{
    internal static class VendrUdiGetterExtensions
    {
        public static GuidUdi GetUdi(this EntityBase entity)
        {
            if (entity is StoreReadOnly store)
                store.GetUdi();

            return null;
        }

        public static GuidUdi GetUdi(this StoreReadOnly entity)
            => new GuidUdi(Constants.UdiEntityType.Store, entity.Id);
    }
}
