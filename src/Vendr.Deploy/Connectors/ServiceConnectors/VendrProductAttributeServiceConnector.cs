﻿using System;
using System.Collections.Generic;
using System.Linq;
using Vendr.Core.Api;
using Vendr.Core.Models;
using Vendr.Deploy.Artifacts;

#if NETFRAMEWORK
using Umbraco.Core;
using Umbraco.Core.Deploy;
#else
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
#endif

namespace Vendr.Deploy.Connectors.ServiceConnectors
{
    [UdiDefinition(VendrConstants.UdiEntityType.ProductAttribute, UdiType.GuidUdi)]
    public class VendrProductAttributeServiceConnector : VendrStoreEntityServiceConnectorBase<ProductAttributeArtifact, ProductAttributeReadOnly, ProductAttribute, ProductAttributeState>
    {
        public override int[] ProcessPasses => new[]
        {
            2
        };

        public override string[] ValidOpenSelectors => new[]
        {
            "this-and-descendants",
            "descendants"
        };

        public override string AllEntitiesRangeName => "ALL VENDR PRODUCT ATTRIBUTE";

        public override string UdiEntityType => VendrConstants.UdiEntityType.ProductAttribute;

        public VendrProductAttributeServiceConnector(IVendrApi vendrApi)
            : base(vendrApi)
        { }

        public override string GetEntityName(ProductAttributeReadOnly entity)
            => entity.Name;

        public override ProductAttributeReadOnly GetEntity(Guid id)
            => _vendrApi.GetProductAttribute(id);

        public override IEnumerable<ProductAttributeReadOnly> GetEntities(Guid storeId)
            => _vendrApi.GetProductAttributes(storeId);

        public override ProductAttributeArtifact GetArtifact(GuidUdi udi, ProductAttributeReadOnly entity)
        {
            if (entity == null)
                return null;

            var storeUdi = new GuidUdi(VendrConstants.UdiEntityType.Store, entity.StoreId);

            var dependencies = new ArtifactDependencyCollection
            {
                new VendrArtifactDependency(storeUdi)
            };

            var artifact = new ProductAttributeArtifact(udi, storeUdi, dependencies)
            {
                Name = new TranslatedValueArtifact<string>
                {
                    Translations = entity.Name.GetTranslatedValues().ToDictionary(x => x.Key, x => x.Value),
                    DefaultValue = entity.Name.GetDefaultValue()
                },
                Code = entity.Alias,
                Values = entity.Values.Select(x => new ProductAttributeValueArtifact
                {
                    Alias = x.Alias,
                    Name = new TranslatedValueArtifact<string>
                    {
                        Translations = x.Name.GetTranslatedValues().ToDictionary(y => y.Key, y => y.Value),
                        DefaultValue = x.Name.GetDefaultValue()
                    }
                }).ToList(),
                SortOrder = entity.SortOrder
            };

            return artifact;
        }

        public override void Process(ArtifactDeployState<ProductAttributeArtifact, ProductAttributeReadOnly> state, IDeployContext context, int pass)
        {
            state.NextPass = GetNextPass(ProcessPasses, pass);

            switch (pass)
            {
                case 2:
                    Pass2(state, context);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(pass));
            }
        }

        private void Pass2(ArtifactDeployState<ProductAttributeArtifact, ProductAttributeReadOnly> state, IDeployContext context)
        {
            using (var uow = _vendrApi.Uow.Create())
            {
                var artifact = state.Artifact;

                artifact.Udi.EnsureType(VendrConstants.UdiEntityType.ProductAttribute);
                artifact.StoreUdi.EnsureType(VendrConstants.UdiEntityType.Store);

                var entity = state.Entity?.AsWritable(uow) ?? ProductAttribute.Create(uow, artifact.Udi.Guid, artifact.StoreUdi.Guid, artifact.Alias, artifact.Name.DefaultValue);

                entity.SetAlias(artifact.Alias)
                    .SetName(new TranslatedValue<string>(artifact.Name.DefaultValue, artifact.Name.Translations))
                    .SetValues(artifact.Values.Select(x => new KeyValuePair<string, TranslatedValue<string>>(x.Alias, new TranslatedValue<string>(x.Name.DefaultValue, x.Name.Translations))))
                    .SetSortOrder(artifact.SortOrder);

                _vendrApi.SaveProductAttribute(entity);

                state.Entity = entity;

                uow.Complete();
            }
        }
    }
}
