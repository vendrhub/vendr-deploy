using Umbraco.Core.Composing;
using Umbraco.Core.Deploy;
using Umbraco.Deploy;
using Umbraco.Deploy.Connectors.ServiceConnectors;
using Vendr.Core.Events;
using Vendr.Core.Models;

namespace Vendr.Deploy.Composing
{
    public partial class VendrDeployComponent : IComponent
    {
        private readonly IDiskEntityService _diskEntityService;
        private readonly IServiceConnectorFactory _serviceConnectorFactory;

        public VendrDeployComponent(IDiskEntityService diskEntityService,
            IServiceConnectorFactory serviceConnectorFactory)
        {
            _diskEntityService = diskEntityService;
            _serviceConnectorFactory = serviceConnectorFactory;
        }

        public void Initialize()
        {
            InitializeFormsDiskRefreshers();
        }

        public void Terminate()
        { }

        private void InitializeFormsDiskRefreshers()
        {
            // Add in Forms Entities as valid Disk entities that can be written out	
            _diskEntityService.RegisterDiskEntityType(VendrConstants.UdiEntityType.Store);
            _diskEntityService.RegisterDiskEntityType(VendrConstants.UdiEntityType.OrderStatus);
            _diskEntityService.RegisterDiskEntityType(VendrConstants.UdiEntityType.EmailTemplate);
            _diskEntityService.RegisterDiskEntityType(VendrConstants.UdiEntityType.TaxClass);

            // TODO: Other entities

            // Stores
            EventHub.NotificationEvents.OnStoreSaved((e) => _diskEntityService.WriteArtifacts(new[] { GetEntityArtifact(e.Store) }));
            EventHub.NotificationEvents.OnStoreDeleted((e) => _diskEntityService.DeleteArtifacts(new[] { GetEntityArtifact(e.Store) }));
            
            // OrderStatus
            EventHub.NotificationEvents.OnOrderStatusSaved((e) => _diskEntityService.WriteArtifacts(new[] { GetEntityArtifact(e.OrderStatus) }));
            EventHub.NotificationEvents.OnOrderStatusDeleted((e) => _diskEntityService.DeleteArtifacts(new[] { GetEntityArtifact(e.OrderStatus) }));

            // EmailTemplate
            EventHub.NotificationEvents.OnEmailTemplateSaved((e) => _diskEntityService.WriteArtifacts(new[] { GetEntityArtifact(e.EmailTemplate) }));
            EventHub.NotificationEvents.OnEmailTemplateDeleted((e) => _diskEntityService.DeleteArtifacts(new[] { GetEntityArtifact(e.EmailTemplate) }));
            
            // EmailTemplate
            EventHub.NotificationEvents.OnTaxClassSaved((e) => _diskEntityService.WriteArtifacts(new[] { GetEntityArtifact(e.TaxClass) }));
            EventHub.NotificationEvents.OnTaxClassDeleted((e) => _diskEntityService.DeleteArtifacts(new[] { GetEntityArtifact(e.TaxClass) }));

            // TODO: Other entity events
        }

        private IArtifact GetEntityArtifact(EntityBase entity)
        {
            var udi = entity.GetUdi();

            return _serviceConnectorFactory
                .GetConnector(udi.EntityType)
                .GetArtifact(entity);
        }
    }
}
