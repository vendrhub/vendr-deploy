using System;
using System.Collections.Generic;
using Vendr.Core.Api;
using Vendr.Deploy.Configuration;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Cms.Core.Models;

namespace Vendr.Deploy.Connectors.ValueConnectors
{
    public class VendrStorePickerValueConnector : IValueConnector
    {
        private readonly IVendrApi _venderApi;
        private readonly VendrDeploySettingsAccessor _settingsAccessor;

        public IEnumerable<string> PropertyEditorAliases => new[] { "Vendr.StorePicker" };

        public VendrStorePickerValueConnector(IVendrApi venderApi, VendrDeploySettingsAccessor settingsAccessor)
        {
            _venderApi = venderApi;
            _settingsAccessor = settingsAccessor;
        }

        public string ToArtifact(object value, IPropertyType propertyType, ICollection<ArtifactDependency> dependencies)
        {
            var svalue = value as string;

            if (string.IsNullOrWhiteSpace(svalue))
                return null;

            if (!Guid.TryParse(svalue, out var storeId))
                return null;

            var store = _venderApi.GetStore(storeId);
            if (store == null)
                return null;

            var udi = new GuidUdi(VendrConstants.UdiEntityType.Store, storeId);

            dependencies.Add(new VendrArtifactDependency(udi));

            return udi.ToString();
        }

        public object FromArtifact(string value, IPropertyType propertyType, object currentValue)
        {
            if (string.IsNullOrWhiteSpace(value) || !UdiHelper.TryParseGuidUdi(value, out var udi) || udi.EntityType != VendrConstants.UdiEntityType.Store)
                return null;

            var store = _venderApi.GetStore(udi.Guid);
            if (store != null)
                return store.Id.ToString();

            return null;
        }
    }
}
