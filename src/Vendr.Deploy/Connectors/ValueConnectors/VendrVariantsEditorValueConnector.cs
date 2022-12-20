using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Vendr.Core.Api;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Serialization;
using Umbraco.Deploy.Core.Connectors.ValueConnectors.Services;
using Umbraco.Deploy.Contrib.ValueConnectors;
using Umbraco.Extensions;
using Microsoft.Extensions.Logging;

namespace Vendr.Deploy.Connectors.ValueConnectors
{
    /// <summary>
    /// A Deploy connector for the Vendr Variants Editor property editor
    /// </summary>
    public class VendrVariantsEditorValueConnector : BlockEditorValueConnector, IValueConnector2
    {
        private readonly IVendrApi _vendrApi;

        public override IEnumerable<string> PropertyEditorAliases => new[] { "Vendr.VariantsEditor" };

        public VendrVariantsEditorValueConnector(IVendrApi vendrApi, 
            IContentTypeService contentTypeService, 
            Lazy<ValueConnectorCollection> valueConnectors,
            ILogger<VendrVariantsEditorValueConnector> logger)
            : base(contentTypeService, valueConnectors, logger)
        {
            _vendrApi = vendrApi;
        }

        public new string ToArtifact(object value, IPropertyType propertyType, ICollection<ArtifactDependency> dependencies, IContextCache contextCache)
        {
            var artifact = base.ToArtifact(value, propertyType, dependencies, contextCache);

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

            var baseValue = JsonConvert.DeserializeObject<BaseValue>(originalVal);
            if (baseValue != null && baseValue.StoreId.HasValue)
            {
                var blockEditorValue = JsonConvert.DeserializeObject<VariantsBlockEditorValue>(artifact);
                if (blockEditorValue == null)
                    return null;

                blockEditorValue.StoreId = baseValue.StoreId.Value;

                var productAttributeAliases = blockEditorValue.Layout.Items.SelectMany(x => x.Config.Attributes.Keys)
                    .Distinct();

                foreach (var productAttributeAlias in productAttributeAliases)
                {
                    var productAttribute = _vendrApi.GetProductAttribute(blockEditorValue.StoreId.Value, productAttributeAlias);
                    if (productAttribute != null)
                    {
                        dependencies.Add(new VendrArtifactDependency(productAttribute.GetUdi()));
                    }
                }

                artifact = JsonConvert.SerializeObject(blockEditorValue);
            }

            return artifact;
        }

        public new object FromArtifact(string value, IPropertyType propertyType, object currentValue, IContextCache contextCache)
        {
            var entity = base.FromArtifact(value, propertyType, currentValue, contextCache);
            var jObj = entity as JObject;
            if (jObj != null && !string.IsNullOrWhiteSpace(value) && value.DetectIsJson())
            {
                var baseValue = JsonConvert.DeserializeObject<BaseValue>(value);
                if (baseValue != null && baseValue.StoreId.HasValue)
                {
                    jObj["storeId"] = baseValue.StoreId.Value;
                }
            }

            return jObj ?? entity;
        }

        object IValueConnector2.FromArtifact(string value, IPropertyType propertyType, object currentValue, IContextCache contextCache)
            => FromArtifact(value, propertyType, currentValue, contextCache);

        string IValueConnector2.ToArtifact(object value, IPropertyType propertyType, ICollection<ArtifactDependency> dependencies, IContextCache contextCache)
            => ToArtifact(value, propertyType, dependencies, contextCache);

        public class BaseValue
        {
            [JsonProperty("storeId")]
            public Guid? StoreId { get; set; }
        }

        public class VariantsBlockEditorValue : BaseValue
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
