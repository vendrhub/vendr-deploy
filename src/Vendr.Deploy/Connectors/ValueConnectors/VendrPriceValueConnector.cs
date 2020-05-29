using System;
using System.Collections.Generic;
using Umbraco.Core.Deploy;
using Umbraco.Core.Models;

namespace Vendr.Deploy.Connectors.ValueConnectors
{
    public class VendrPriceValueConnector : IValueConnector
    {
        public IEnumerable<string> PropertyEditorAliases => new[] { "Vendr.Price" };

        public object FromArtifact(string value, PropertyType propertyType, object currentValue)
        {
            throw new NotImplementedException();
        }

        public string ToArtifact(object value, PropertyType propertyType, ICollection<ArtifactDependency> dependencies)
        {
            throw new NotImplementedException();
        }
    }
}
