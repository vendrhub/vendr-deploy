using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Deploy;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Serialization;
using Umbraco.Core.Services;
using Umbraco.Deploy.Connectors.ValueConnectors.Services;
using Umbraco.Deploy.Contrib.Connectors.ValueConnectors;
using Vendr.Core.Api;

namespace Vendr.Deploy.Connectors.ValueConnectors
{
    /// <summary>
    /// A Deploy connector for the Vendr Variants Editor property editor
    /// </summary>
    public class VendrVariantsEditorValueConnector : BlockEditorValueConnector, IValueConnector
    {
        private readonly IVendrApi _venderApi;

        public override IEnumerable<string> PropertyEditorAliases => new[] { "Vendr.VariantsEditor" };

        public VendrVariantsEditorValueConnector(IVendrApi vendrApi, IContentTypeService contentTypeService, 
            Lazy<ValueConnectorCollection> valueConnectors, ILogger logger)
            : base(contentTypeService, valueConnectors, logger)
        {
            _venderApi = vendrApi;
        }

        public new string ToArtifact(object value, PropertyType propertyType, ICollection<ArtifactDependency> dependencies)
        {
            var artifact = base.ToArtifact(value, propertyType, dependencies);

            if (string.IsNullOrWhiteSpace(artifact) || !artifact.DetectIsJson())
                return null;

            // The base call to ToArtifact will have stripped off the storeId property
            // held in the root of the property value so we need to re-parse the original
            // value and extract the store ID. If one is present, then we need to append
            // this back into the artifact value but also then attempt to process any
            // product attributes that need deploying.

            var originalVal = value is JObject
                ? value.ToString()
                : value as string;

            var storeValue = JsonConvert.DeserializeObject<StoreValue>(originalVal);
            if (storeValue != null && storeValue.StoreId.HasValue)
            {
                var blockEditorValue = JsonConvert.DeserializeObject<VariantsBlockEditorValue>(artifact);
                if (blockEditorValue == null)
                    return null;

                blockEditorValue.StoreId = storeValue.StoreId.Value;

                var productAttributeAliases = blockEditorValue.Layout.Items.SelectMany(x => x.Config.Attributes.Keys)
                    .Distinct();

                foreach (var productAttributeAlias in productAttributeAliases)
                {
                    var productAttribute = _venderApi.GetProductAttribute(blockEditorValue.StoreId.Value, productAttributeAlias);
                    if (productAttribute != null)
                    {
                        dependencies.Add(new ArtifactDependency(productAttribute.GetUdi(), false, ArtifactDependencyMode.Match));
                    }
                }

                artifact = JsonConvert.SerializeObject(blockEditorValue);
            }

            return artifact;
        }

        public new object FromArtifact(string value, PropertyType propertyType, object currentValue)
        {
            var entity = base.FromArtifact(value, propertyType, currentValue);

            // The base call to FromAtrtifact will have stripped off the storeId property
            // held in the root of the property value so we need to re-parse the original
            // value and extract the store ID, appending it back onto the JObject

            var jObj = entity as JObject;
            if (jObj != null && !string.IsNullOrWhiteSpace(value) && value.DetectIsJson())
            {
                var storeValue = JsonConvert.DeserializeObject<StoreValue>(value);
                if (storeValue != null && storeValue.StoreId.HasValue)
                {
                    jObj["storeId"] = storeValue.StoreId.Value;
                }
            }

            return entity;
        }

        object IValueConnector.FromArtifact(string value, PropertyType propertyType, object currentValue)
            => FromArtifact(value, propertyType, currentValue);

        string IValueConnector.ToArtifact(object value, PropertyType propertyType, ICollection<ArtifactDependency> dependencies)
            => ToArtifact(value, propertyType, dependencies);

        public class StoreValue
        {
            [JsonProperty("storeId")]
            public Guid? StoreId { get; set; }
        }

        public class VariantsBlockEditorValue : StoreValue
        {
            [JsonProperty("layout")]
            public VariantsBlockEditorLayout Layout { get; set; }

            [JsonProperty("contentData")]
            public IEnumerable<Block> Content { get; set; }

            [JsonProperty("settingsData")]
            public IEnumerable<Block> Settings { get; set; }
        }

        public class VariantsBlockEditorLayout
        {
            [JsonProperty("Vendr.VariantsEditor")]
            public IEnumerable<VariantsBlockEditorLayoutItem> Items { get; set; }
        }

        public class VariantsBlockEditorLayoutItem
        {
            [JsonProperty("contentUdi")]
            [JsonConverter(typeof(UdiJsonConverter))]
            public Udi ContentUdi { get; set; }

            [JsonProperty("config")]
            public ProductVariantConfig Config { get; set; }
        }

        public class ProductVariantConfig
        {
            [JsonProperty("attributes")]
            public IDictionary<string, string> Attributes { get; set; }

            [JsonProperty("isDefault")]
            public bool IsDefault { get; set; }
        }
    }
}
