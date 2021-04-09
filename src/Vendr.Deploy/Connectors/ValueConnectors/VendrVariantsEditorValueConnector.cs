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
using Vendr.Core.Models;
using Vendr.Deploy.Artifacts;
using Vendr.Deploy.Connectors.ServiceConnectors;

namespace Vendr.Deploy.Connectors.ValueConnectors
{
    /// <summary>
    /// A Deploy connector for the Vendr Variants Editor property editor
    /// </summary>
    public class VendrVariantsEditorValueConnector : BlockEditorValueConnector, IValueConnector
    {
        private readonly IVendrApi _vendrApi;
        private readonly VendrProductAttributeServiceConnector _productAttributeServiceConnector;

        public override IEnumerable<string> PropertyEditorAliases => new[] { "Vendr.VariantsEditor" };

        public VendrVariantsEditorValueConnector(IVendrApi vendrApi, 
            IContentTypeService contentTypeService, Lazy<ValueConnectorCollection> valueConnectors, ILogger logger)
            : base(contentTypeService, valueConnectors, logger)
        {
            _vendrApi = vendrApi;
            _productAttributeServiceConnector = new VendrProductAttributeServiceConnector(vendrApi);
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

            var baseValue = JsonConvert.DeserializeObject<BaseValue>(originalVal);
            if (baseValue != null && baseValue.StoreId.HasValue)
            {
                var blockEditorValue = JsonConvert.DeserializeObject<VariantsBlockEditorValue>(artifact);
                if (blockEditorValue == null)
                    return null;

                blockEditorValue.StoreId = baseValue.StoreId.Value;

                var productAttributeAliases = blockEditorValue.Layout.Items.SelectMany(x => x.Config.Attributes.Keys)
                    .Distinct();

                var productAttributeArtifacts = new List<ProductAttributeArtifact>();

                foreach (var productAttributeAlias in productAttributeAliases)
                {
                    var productAttribute = _vendrApi.GetProductAttribute(blockEditorValue.StoreId.Value, productAttributeAlias);
                    if (productAttribute != null)
                    {
                        var paArtifact = _productAttributeServiceConnector.GetArtifact(productAttribute.GetUdi(), productAttribute);

                        productAttributeArtifacts.Add(paArtifact);
                    }
                }

                blockEditorValue.ProductAttributes = productAttributeArtifacts;

                artifact = JsonConvert.SerializeObject(blockEditorValue);
            }

            return artifact;
        }

        public new object FromArtifact(string value, PropertyType propertyType, object currentValue)
        {
            BaseValue baseValue = null;

            if (!string.IsNullOrWhiteSpace(value) && value.DetectIsJson())
            {
                baseValue = JsonConvert.DeserializeObject<BaseValue>(value);

                // We can't currently deploy custom entities via deploy so we fudge it by appending the
                // product attributes to the serialized data or of property value. We then process
                // these attributes ourselves

                if (baseValue.ProductAttributes != null)
                {
                    using (var uow = _vendrApi.Uow.Create())
                    {
                        foreach (var artifact in baseValue.ProductAttributes)
                        {
                            artifact.Udi.EnsureType(VendrConstants.UdiEntityType.ProductAttribute);
                            artifact.StoreUdi.EnsureType(VendrConstants.UdiEntityType.Store);

                            var attrEntity = _vendrApi.GetProductAttribute(artifact.Udi.Guid)?.AsWritable(uow) ?? ProductAttribute.Create(uow, artifact.Udi.Guid, artifact.StoreUdi.Guid, artifact.Alias, artifact.Name.DefaultValue);

                            attrEntity.SetAlias(artifact.Alias)
                                .SetName(new TranslatedValue<string>(artifact.Name.DefaultValue, artifact.Name.Translations))
                                .SetValues(artifact.Values.Select(x => new KeyValuePair<string, TranslatedValue<string>>(x.Alias, new TranslatedValue<string>(x.Name.DefaultValue, x.Name.Translations))))
                                .SetSortOrder(artifact.SortOrder);

                            _vendrApi.SaveProductAttribute(attrEntity);
                        }

                        uow.Complete();
                    }

                    
                }
            }

            var entity = base.FromArtifact(value, propertyType, currentValue);

            // The base call to FromAtrtifact will have stripped off the storeId property
            // held in the root of the property value so we need to re-parse the original
            // value and extract the store ID, appending it back onto the JObject

            var jObj = entity as JObject;
            if (jObj != null && baseValue != null && baseValue.StoreId.HasValue)
            {
                jObj["storeId"] = baseValue.StoreId.Value;
            }

            return entity;
        }

        object IValueConnector.FromArtifact(string value, PropertyType propertyType, object currentValue)
            => FromArtifact(value, propertyType, currentValue);

        string IValueConnector.ToArtifact(object value, PropertyType propertyType, ICollection<ArtifactDependency> dependencies)
            => ToArtifact(value, propertyType, dependencies);

        public class BaseValue
        {
            [JsonProperty("storeId")]
            public Guid? StoreId { get; set; }

            [JsonProperty("productAttributes")]
            public IEnumerable<ProductAttributeArtifact> ProductAttributes { get; set; }
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
