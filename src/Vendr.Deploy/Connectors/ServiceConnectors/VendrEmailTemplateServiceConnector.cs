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
    [UdiDefinition(VendrConstants.UdiEntityType.EmailTemplate, UdiType.GuidUdi)]
    public class VendrEmailTemplateServiceConnector : VendrStoreEntityServiceConnectorBase<EmailTemplateArtifact, EmailTemplateReadOnly, EmailTemplate, EmailTemplateState>
    {
        public override int[] ProcessPasses => new[]
        {
            2
        };

        public override string[] ValidOpenSelectors => new[]
        {
            "this",
            "this-and-descendants",
            "descendants"
        };

        public override string AllEntitiesRangeName => "All Vendr Email Templates";

        public override string UdiEntityType => VendrConstants.UdiEntityType.EmailTemplate;

        public VendrEmailTemplateServiceConnector(IVendrApi vendrApi, VendrDeploySettingsAccessor settingsAccessor)
            : base(vendrApi, settingsAccessor)
        { }

        public override string GetEntityName(EmailTemplateReadOnly entity)
            => entity.Name;

        public override EmailTemplateReadOnly GetEntity(Guid id)
            => _vendrApi.GetEmailTemplate(id);

        public override IEnumerable<EmailTemplateReadOnly> GetEntities(Guid storeId)
            => _vendrApi.GetEmailTemplates(storeId);

        public override EmailTemplateArtifact GetArtifact(GuidUdi udi, EmailTemplateReadOnly entity)
        {
            if (entity == null)
                return null;

            var storeUdi = new GuidUdi(VendrConstants.UdiEntityType.Store, entity.StoreId);

            var dependencies = new ArtifactDependencyCollection
            {
                new VendrArtifactDependency(storeUdi)
            };

            return new EmailTemplateArtifact(udi, storeUdi, dependencies)
            {
                Name = entity.Name,
                Alias = entity.Alias,
                Category = (int)entity.Category,
                Subject = entity.Subject,
                SenderName = entity.SenderName,
                SenderAddress = entity.SenderAddress,
                ToAddresses = entity.ToAddresses,
                CcAddresses = entity.CcAddresses,
                BccAddresses = entity.BccAddresses,
                SendToCustomer = entity.SendToCustomer,
                TemplateView = entity.TemplateView,
                SortOrder = entity.SortOrder
            };
        }

        public override void Process(ArtifactDeployState<EmailTemplateArtifact, EmailTemplateReadOnly> state, IDeployContext context, int pass)
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

        private void Pass2(ArtifactDeployState<EmailTemplateArtifact, EmailTemplateReadOnly> state, IDeployContext context)
        {
            _vendrApi.Uow.Execute(uow =>
            {
                var artifact = state.Artifact;

                artifact.Udi.EnsureType(VendrConstants.UdiEntityType.EmailTemplate);
                artifact.StoreUdi.EnsureType(VendrConstants.UdiEntityType.Store);

                var entity = state.Entity?.AsWritable(uow) ?? EmailTemplate.Create(uow, artifact.Udi.Guid, artifact.StoreUdi.Guid, artifact.Alias, artifact.Name);

                entity.SetName(artifact.Name, artifact.Alias)
                    .SetCategory((TemplateCategory)artifact.Category)
                    .SetSendToCustomer(artifact.SendToCustomer)
                    .SetSubject(artifact.Subject)
                    .SetSender(artifact.SenderName, artifact.SenderAddress)
                    .SetToAddresses(artifact.ToAddresses)
                    .SetCcAddresses(artifact.CcAddresses)
                    .SetBccAddresses(artifact.BccAddresses)
                    .SetTemplateView(artifact.TemplateView)
                    .SetSortOrder(artifact.SortOrder);

                _vendrApi.SaveEmailTemplate(entity);

                uow.Complete();
            });
        }
    }
}
