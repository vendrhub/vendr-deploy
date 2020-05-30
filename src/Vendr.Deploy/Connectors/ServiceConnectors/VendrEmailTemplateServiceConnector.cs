using System;
using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Deploy;
using Vendr.Core.Api;
using Vendr.Core.Models;
using Vendr.Deploy.Artifacts;

namespace Vendr.Deploy.Connectors.ServiceConnectors
{
    [UdiDefinition(Constants.UdiEntityType.EmailTemplate, UdiType.GuidUdi)]
    public class VendrEmailTemplateServiceConnector : VendrStoreEntityServiceConnectorBase<EmailTemplateArtifact, EmailTemplateReadOnly, EmailTemplate, EmailTemplateState>
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

        public override string AllEntitiesRangeName => "ALL VENDR EMAIL TEMPLATE";

        public override string UdiEntityType => Constants.UdiEntityType.EmailTemplate;

        public VendrEmailTemplateServiceConnector(IVendrApi vendrApi)
            : base(vendrApi)
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

            var storeUdi = new GuidUdi(Constants.UdiEntityType.Store, entity.StoreId);

            var dependencies = new ArtifactDependencyCollection
            {
                new ArtifactDependency(storeUdi, true, ArtifactDependencyMode.Match)
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
            using (var uow = _vendrApi.Uow.Create())
            {
                var artifact = state.Artifact;
                var entity = state.Entity?.AsWritable(uow) ?? EmailTemplate.Create(uow, artifact.Udi.Guid, artifact.StoreId.Guid, artifact.Alias, artifact.Name);

                entity.SetName(artifact.Name, artifact.Alias)
                    .SetCategory((EmailTemplateCategory)artifact.Category)
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
            }
        }
    }
}
