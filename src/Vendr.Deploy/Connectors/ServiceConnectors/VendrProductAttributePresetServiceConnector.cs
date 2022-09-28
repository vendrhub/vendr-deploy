using System;
using System.Collections.Generic;
using Vendr.Core.Api;
using Vendr.Core.Models;
using Vendr.Deploy.Artifacts;
using Vendr.Deploy.Configuration;

using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using System.Linq;
using Umbraco.Extensions;

namespace Vendr.Deploy.Connectors.ServiceConnectors
{
    [UdiDefinition(VendrConstants.UdiEntityType.ProductAttributePreset, UdiType.GuidUdi)]
    public class VendrProductAttributePresetServiceConnector : VendrStoreEntityServiceConnectorBase<ProductAttributePresetArtifact, ProductAttributePresetReadOnly, ProductAttributePreset, ProductAttributePresetState>
    {
        public override int[] ProcessPasses => new[]
        {
            2,4
        };

        public override string[] ValidOpenSelectors => new[]
        {
            "this",
            "this-and-descendants",
            "descendants"
        };

        public override string AllEntitiesRangeName => "All Vendr Product Attribute Presets";

        public override string UdiEntityType => VendrConstants.UdiEntityType.ProductAttributePreset;

        public override string ContainerId => Umbraco.Constants.Trees.Stores.Ids[Umbraco.Constants.Trees.Stores.NodeType.ProductAttributePresets].ToInvariantString();

        public VendrProductAttributePresetServiceConnector(IVendrApi vendrApi, VendrDeploySettingsAccessor settingsAccessor)
            : base(vendrApi, settingsAccessor)
        { }

        public override string GetEntityName(ProductAttributePresetReadOnly entity)
            => entity.Name;

        public override ProductAttributePresetReadOnly GetEntity(Guid id)
            => _vendrApi.GetProductAttributePreset(id);

        public override IEnumerable<ProductAttributePresetReadOnly> GetEntities(Guid storeId)
            => _vendrApi.GetProductAttributePresets(storeId);

        public override ProductAttributePresetArtifact GetArtifact(GuidUdi udi, ProductAttributePresetReadOnly entity)
        {
            if (entity == null)
                return null;

            var storeUdi = new GuidUdi(VendrConstants.UdiEntityType.Store, entity.StoreId);

            var dependencies = new ArtifactDependencyCollection
            {
                new VendrArtifactDependency(storeUdi)
            };

            var allowedAttributes = new List<AllowedProductAttributeArtifact>();

            foreach (var allowedAttr in entity.AllowedAttributes)
            {
                // Get product attribute ID
                var attr = _vendrApi.GetProductAttribute(entity.StoreId, allowedAttr.ProductAttributeAlias);
                var attrUdi = new GuidUdi(VendrConstants.UdiEntityType.ProductAttribute, attr.Id);

                // Add the product attribute as a dependency
                dependencies.Add(new VendrArtifactDependency(attrUdi));

                // Add the allowed attribute to the collection of attributes
                allowedAttributes.Add(new AllowedProductAttributeArtifact
                {
                    ProductAttributeUdi = attrUdi,
                    AllowedValueAliases = allowedAttr.AllowedValueAliases
                });
            }

            var artifact = new ProductAttributePresetArtifact(udi, storeUdi, dependencies)
            {
                Name = entity.Name,
                Code = entity.Alias,
                AllowedAttributes = allowedAttributes,
                SortOrder = entity.SortOrder
            };

            return artifact;
        }

        public override void Process(ArtifactDeployState<ProductAttributePresetArtifact, ProductAttributePresetReadOnly> state, IDeployContext context, int pass)
        {
            state.NextPass = GetNextPass(ProcessPasses, pass);

            switch (pass)
            {
                case 2:
                    Pass2(state, context);
                    break;
                case 4:
                    Pass4(state, context);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(pass));
            }
        }

        private void Pass2(ArtifactDeployState<ProductAttributePresetArtifact, ProductAttributePresetReadOnly> state, IDeployContext context)
        {
            _vendrApi.Uow.Execute(uow =>
            {
                var artifact = state.Artifact;

                artifact.Udi.EnsureType(VendrConstants.UdiEntityType.ProductAttributePreset);
                artifact.StoreUdi.EnsureType(VendrConstants.UdiEntityType.Store);

                var entity = state.Entity?.AsWritable(uow) ?? ProductAttributePreset.Create(uow, artifact.Udi.Guid, artifact.StoreUdi.Guid, artifact.Alias, artifact.Name);

                entity.SetAlias(artifact.Alias)
                    .SetName(artifact.Name)
                    .SetSortOrder(artifact.SortOrder);

                _vendrApi.SaveProductAttributePreset(entity);

                state.Entity = entity;

                uow.Complete();
            });
        }

        private void Pass4(ArtifactDeployState<ProductAttributePresetArtifact, ProductAttributePresetReadOnly> state, IDeployContext context)
        {
            _vendrApi.Uow.Execute(uow =>
            {
                var artifact = state.Artifact;
                var entity = _vendrApi.GetProductAttributePreset(state.Entity.Id).AsWritable(uow);

                var productAttributeAliasMap = _vendrApi.GetProductAttributes(artifact.AllowedAttributes.Select(x => x.ProductAttributeUdi.Guid).ToArray())
                    .ToDictionary(x => x.Id, x => x);

                var allowedAttributes = new List<AllowedProductAttribute>();

                foreach (var allowedAttr in artifact.AllowedAttributes.Where(x => productAttributeAliasMap.ContainsKey(x.ProductAttributeUdi.Guid)))
                {
                    var attr = productAttributeAliasMap[allowedAttr.ProductAttributeUdi.Guid];

                    allowedAttributes.Add(new AllowedProductAttribute(attr.Alias, 
                        allowedAttr.AllowedValueAliases
                            .Where(x => attr.Values.Any(y => y.Alias.InvariantEquals(x)))
                            .ToList()));
                }

                entity.SetAllowedAttributes(allowedAttributes, SetBehavior.Replace);

                _vendrApi.SaveProductAttributePreset(entity);

                uow.Complete();
            });
        }
    }
}
