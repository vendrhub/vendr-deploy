using System;
using System.Collections.Generic;
using Vendr.Core.Api;
using Vendr.Core.Models;
using Vendr.Deploy.Artifacts;
using Vendr.Deploy.Configuration;

using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;

namespace Vendr.Deploy.Connectors.ServiceConnectors
{
    [UdiDefinition(VendrConstants.UdiEntityType.PrintTemplate, UdiType.GuidUdi)]
    public class VendrPrintTemplateServiceConnector : VendrStoreEntityServiceConnectorBase<PrintTemplateArtifact, PrintTemplateReadOnly, PrintTemplate, PrintTemplateState>
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

        public override string AllEntitiesRangeName => "ALL VENDR PRINT TEMPLATE";

        public override string UdiEntityType => VendrConstants.UdiEntityType.PrintTemplate;

        public VendrPrintTemplateServiceConnector(IVendrApi vendrApi, VendrDeploySettingsAccessor settingsAccessor)
            : base(vendrApi, settingsAccessor)
        { }

        public override string GetEntityName(PrintTemplateReadOnly entity)
            => entity.Name;

        public override PrintTemplateReadOnly GetEntity(Guid id)
            => _vendrApi.GetPrintTemplate(id);

        public override IEnumerable<PrintTemplateReadOnly> GetEntities(Guid storeId)
            => _vendrApi.GetPrintTemplates(storeId);

        public override PrintTemplateArtifact GetArtifact(GuidUdi udi, PrintTemplateReadOnly entity)
        {
            if (entity == null)
                return null;

            var storeUdi = new GuidUdi(VendrConstants.UdiEntityType.Store, entity.StoreId);

            var dependencies = new ArtifactDependencyCollection
            {
                new VendrArtifactDependency(storeUdi)
            };

            return new PrintTemplateArtifact(udi, storeUdi, dependencies)
            {
                Name = entity.Name,
                Alias = entity.Alias,
                Category = (int)entity.Category,
                TemplateView = entity.TemplateView,
                SortOrder = entity.SortOrder
            };
        }

        public override void Process(ArtifactDeployState<PrintTemplateArtifact, PrintTemplateReadOnly> state, IDeployContext context, int pass)
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

        private void Pass2(ArtifactDeployState<PrintTemplateArtifact, PrintTemplateReadOnly> state, IDeployContext context)
        {
            _vendrApi.Uow.Execute(uow =>
            {
                var artifact = state.Artifact;

                artifact.Udi.EnsureType(VendrConstants.UdiEntityType.PrintTemplate);
                artifact.StoreUdi.EnsureType(VendrConstants.UdiEntityType.Store);

                var entity = state.Entity?.AsWritable(uow) ?? PrintTemplate.Create(uow, artifact.Udi.Guid, artifact.StoreUdi.Guid, artifact.Alias, artifact.Name);

                entity.SetName(artifact.Name, artifact.Alias)
                    .SetCategory((TemplateCategory)artifact.Category)
                    .SetTemplateView(artifact.TemplateView)
                    .SetSortOrder(artifact.SortOrder);

                _vendrApi.SavePrintTemplate(entity);

                uow.Complete();
            });
        }
    }
}
