using System;
using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Deploy;
using Umbraco.Core.Models;
using Vendr.Core.Api;

namespace Vendr.Deploy.Connectors.ValueConnectors
{
    public class VendrStorePickerValueConnector : IValueConnector
    {
        private readonly IVendrApi _venderApi;

        public IEnumerable<string> PropertyEditorAliases => new[] { "Vendr.StorePicker" };

        public VendrStorePickerValueConnector(IVendrApi venderApi)
        {
            _venderApi = venderApi;
        }

        public string ToArtifact(object value, PropertyType propertyType, ICollection<ArtifactDependency> dependencies)
        {
            var svalue = value as string;

            if (string.IsNullOrWhiteSpace(svalue))
                return null;

            if (!Guid.TryParse(svalue, out var storeId))
                return null;

            var store = _venderApi.GetStore(storeId);
            if (store == null)
                return null;

            var udi = new GuidUdi(Constants.UdiEntityType.Store, storeId);

            dependencies.Add(new ArtifactDependency(udi, false, ArtifactDependencyMode.Exist));

            return udi.ToString();
        }

        public object FromArtifact(string value, PropertyType propertyType, object currentValue)
        {
            if (string.IsNullOrWhiteSpace(value) || !GuidUdi.TryParse(value, out var udi) || udi.EntityType != Constants.UdiEntityType.Store)
                return null;

            var store = _venderApi.GetStore(udi.Guid);
            if (store != null)
                return store.Id.ToString();

            return null;
        }
    }
}
