using System;
using System.Collections.Generic;
using Umbraco.Core.Deploy;
using Umbraco.Core.Models;
using Vendr.Core.Api;

namespace Vendr.Deploy.Connectors.ValueConnectors
{
    public class VendrStoreEntityPickerValueConnector : IValueConnector
    {
        private readonly IVendrApi _venderApi;

        public IEnumerable<string> PropertyEditorAliases => new[] { "Vendr.StoreEntityPicker" };

        public VendrStoreEntityPickerValueConnector(IVendrApi venderApi)
        {
            _venderApi = venderApi;
        }

        public string ToArtifact(object value, PropertyType propertyType, ICollection<ArtifactDependency> dependencies)
        {
            throw new NotImplementedException();
        }

        public object FromArtifact(string value, PropertyType propertyType, object currentValue)
        {
            throw new NotImplementedException();
        }
    }
}
