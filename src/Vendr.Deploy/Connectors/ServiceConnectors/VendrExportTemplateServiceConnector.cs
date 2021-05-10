using System;
using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Deploy;
using Vendr.Core.Api;
using Vendr.Core.Models;
using Vendr.Deploy.Artifacts;

namespace Vendr.Deploy.Connectors.ServiceConnectors
{
    [UdiDefinition(VendrConstants.UdiEntityType.ExportTemplate, UdiType.GuidUdi)]
    public class VendrExportTemplateServiceConnector : VendrStoreEntityServiceConnectorBase<ExportTemplateArtifact, ExportTemplateReadOnly, ExportTemplate, ExportTemplateState>
    {
        public override int[] ProcessPasses => new [] 
        {
            2
        };

        public override string[] ValidOpenSelectors => new []
        {
            "this-and-descendants",
            "descendants"
        };

        public override string AllEntitiesRangeName => "ALL VENDR EXPORT TEMPLATE";

        public override string UdiEntityType => VendrConstants.UdiEntityType.ExportTemplate;

        public VendrExportTemplateServiceConnector(IVendrApi vendrApi)
            : base(vendrApi)
        { }

        public override string GetEntityName(ExportTemplateReadOnly entity)
            => entity.Name;

        public override ExportTemplateReadOnly GetEntity(Guid id)
            => _vendrApi.GetExportTemplate(id);

        public override IEnumerable<ExportTemplateReadOnly> GetEntities(Guid storeId)
            => _vendrApi.GetExportTemplates(storeId);

        public override ExportTemplateArtifact GetArtifact(GuidUdi udi, ExportTemplateReadOnly entity)
        {
            if (entity == null)
                return null;

            var storeUdi = new GuidUdi(VendrConstants.UdiEntityType.Store, entity.StoreId);

            var dependencies = new ArtifactDependencyCollection
            {
                new VendrArtifactDependency(storeUdi)
            };

            return new ExportTemplateArtifact(udi, storeUdi, dependencies)
            {
                Name = entity.Name,
                Alias = entity.Alias,
                Category = (int)entity.Category,
                FileMimeType = entity.FileMimeType,
                FileExtension = entity.FileExtension,
                ExportStrategy = (int)entity.ExportStrategy,
                TemplateView = entity.TemplateView,
                SortOrder = entity.SortOrder
            };
        }

        public override void Process(ArtifactDeployState<ExportTemplateArtifact, ExportTemplateReadOnly> state, IDeployContext context, int pass)
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

        private void Pass2(ArtifactDeployState<ExportTemplateArtifact, ExportTemplateReadOnly> state, IDeployContext context)
        {
            using (var uow = _vendrApi.Uow.Create())
            {
                var artifact = state.Artifact;

                artifact.Udi.EnsureType(VendrConstants.UdiEntityType.ExportTemplate);
                artifact.StoreUdi.EnsureType(VendrConstants.UdiEntityType.Store);

                var entity = state.Entity?.AsWritable(uow) ?? ExportTemplate.Create(uow, artifact.Udi.Guid, artifact.StoreUdi.Guid, artifact.Alias, artifact.Name);

                entity.SetName(artifact.Name, artifact.Alias)
                    .SetCategory((TemplateCategory)artifact.Category)
                    .SetFileMimeType(artifact.FileMimeType)
                    .SetFileExtension(artifact.FileExtension)
                    .SetExportStrategy((ExportStrategy)artifact.ExportStrategy)
                    .SetTemplateView(artifact.TemplateView)
                    .SetSortOrder(artifact.SortOrder);

                _vendrApi.SaveExportTemplate(entity);

                uow.Complete();
            }
        }
    }
}
