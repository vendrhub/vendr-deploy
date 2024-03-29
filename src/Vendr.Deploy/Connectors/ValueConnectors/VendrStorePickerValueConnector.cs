﻿using System;
using System.Collections.Generic;
using Vendr.Core.Api;

#if NETFRAMEWORK
using Umbraco.Core;
using Umbraco.Core.Deploy;
using IPropertyType = Umbraco.Core.Models.PropertyType;
#else
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Cms.Core.Models;
#endif

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

            dependencies.Add(new VendrArtifactDependency(udi, ArtifactDependencyMode.Exist));

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
