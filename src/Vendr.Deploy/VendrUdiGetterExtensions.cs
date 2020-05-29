using Umbraco.Core;
using Vendr.Core.Models;

namespace Vendr.Deploy
{
    internal static class VendrUdiGetterExtensions
    {
        public static GuidUdi GetUdi(this StoreReadOnly entity)
            => new GuidUdi("vendr-store", entity.Id);
    }
}
