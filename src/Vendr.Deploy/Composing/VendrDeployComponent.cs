using Vendr.Core.Events;
using Vendr.Core.Models;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Deploy.Infrastructure.Disk;
using Umbraco.Deploy.Core.Connectors.ServiceConnectors;
using Umbraco.Deploy.Infrastructure.Transfer;
using Microsoft.AspNetCore.Http;
using System;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Extensions;

using StaticServiceProvider = Umbraco.Cms.Web.Common.DependencyInjection.StaticServiceProvider;

namespace Vendr.Deploy.Composing
{
    public partial class VendrDeployComponent : IComponent
    {
        private readonly IDiskEntityService _diskEntityService;
        private readonly IServiceConnectorFactory _serviceConnectorFactory;
        private readonly ITransferEntityService _transferEntityService;

        public VendrDeployComponent(IDiskEntityService diskEntityService,
            IServiceConnectorFactory serviceConnectorFactory,
            ITransferEntityService transferEntityService)
        {
            _diskEntityService = diskEntityService;
            _serviceConnectorFactory = serviceConnectorFactory;
            _transferEntityService = transferEntityService;
        }

        public void Initialize()
        {
            InitializeDiskRefreshers();
            InitializeIntegratedEntities();
        }

        public void Terminate()
        { }

        private void InitializeIntegratedEntities()
        {
            // Add in integrated transfer entities
            _transferEntityService.RegisterTransferEntityType(
                VendrConstants.UdiEntityType.ProductAttribute,
                "Product Attributes",
                new DeployRegisteredEntityTypeDetailOptions
                {
                    SupportsQueueForTransfer = true,
                    SupportsQueueForTransferOfDescendents = true,
                    SupportsRestore = true,
                    PermittedToRestore = true,
                    SupportsPartialRestore = true,
                },
                false,
                Umbraco.Constants.Trees.Stores.Alias,
                (string routePath) => Regex.IsMatch(routePath, $"^-1/[^/]+/10/11"),
                (string nodeId) =>
                {
                    var httpContext = StaticServiceProvider.Instance.GetRequiredService<IHttpContextAccessor>().HttpContext;
                    var nodeType = httpContext.Request.Query["nodeType"].ToString();

                    return nodeType.InvariantEquals(Umbraco.Constants.Trees.Stores.NodeType.ProductAttributes.ToString())
                        || nodeType.InvariantEquals(Umbraco.Constants.Trees.Stores.NodeType.ProductAttribute.ToString());
                },
                (string nodeId, HttpContext httpContext, out Guid entityId) => Guid.TryParse(nodeId, out entityId));
                // TODO: , new DeployTransferRegisteredEntityTypeDetail.RemoteTreeDetail(FormsTreeHelper.GetExampleTree, "example", "externalExampleTree"));

            _transferEntityService.RegisterTransferEntityType(
                VendrConstants.UdiEntityType.ProductAttributePreset,
                "Product Attribute Presets",
                new DeployRegisteredEntityTypeDetailOptions
                {
                    SupportsQueueForTransfer = true,
                    SupportsQueueForTransferOfDescendents = true,
                    SupportsRestore = true,
                    PermittedToRestore = true,
                    SupportsPartialRestore = true,
                },
                false,
                Umbraco.Constants.Trees.Stores.Alias,
                (string routePath) => Regex.IsMatch(routePath, $"^-1/[^/]+/10/12"),
                (string nodeId) =>
                {
                    var httpContext = StaticServiceProvider.Instance.GetRequiredService<IHttpContextAccessor>().HttpContext;
                    var nodeType = httpContext.Request.Query["nodeType"].ToString();

                    return nodeType.InvariantEquals(Umbraco.Constants.Trees.Stores.NodeType.ProductAttributePresets.ToString())
                        || nodeType.InvariantEquals(Umbraco.Constants.Trees.Stores.NodeType.ProductAttributePreset.ToString());
                },
                (string nodeId, HttpContext httpContext, out Guid entityId) => Guid.TryParse(nodeId, out entityId));
                // TODO: , new DeployTransferRegisteredEntityTypeDetail.RemoteTreeDetail(FormsTreeHelper.GetExampleTree, "example", "externalExampleTree"));
        }

        private void InitializeDiskRefreshers()
        {
            // Add in settings entities as valid Disk entities that can be written out	
            _diskEntityService.RegisterDiskEntityType(VendrConstants.UdiEntityType.Store);
            _diskEntityService.RegisterDiskEntityType(VendrConstants.UdiEntityType.OrderStatus);
            _diskEntityService.RegisterDiskEntityType(VendrConstants.UdiEntityType.ShippingMethod);
            _diskEntityService.RegisterDiskEntityType(VendrConstants.UdiEntityType.PaymentMethod);
            _diskEntityService.RegisterDiskEntityType(VendrConstants.UdiEntityType.Country);
            _diskEntityService.RegisterDiskEntityType(VendrConstants.UdiEntityType.Region);
            _diskEntityService.RegisterDiskEntityType(VendrConstants.UdiEntityType.Currency);
            _diskEntityService.RegisterDiskEntityType(VendrConstants.UdiEntityType.TaxClass);
            _diskEntityService.RegisterDiskEntityType(VendrConstants.UdiEntityType.EmailTemplate);
            _diskEntityService.RegisterDiskEntityType(VendrConstants.UdiEntityType.PrintTemplate);
            _diskEntityService.RegisterDiskEntityType(VendrConstants.UdiEntityType.ExportTemplate);

            // TODO: Other entities

            // Stores
            EventHub.NotificationEvents.OnStoreSaved((e) => _diskEntityService.WriteArtifacts(new[] { GetEntityArtifact(e.Store) }));
            EventHub.NotificationEvents.OnStoreDeleted((e) => _diskEntityService.DeleteArtifacts(new[] { GetEntityArtifact(e.Store) }));

            // OrderStatus
            EventHub.NotificationEvents.OnOrderStatusSaved((e) => _diskEntityService.WriteArtifacts(new[] { GetEntityArtifact(e.OrderStatus) }));
            EventHub.NotificationEvents.OnOrderStatusDeleted((e) => _diskEntityService.DeleteArtifacts(new[] { GetEntityArtifact(e.OrderStatus) }));

            // ShippingMethod
            EventHub.NotificationEvents.OnShippingMethodSaved((e) => _diskEntityService.WriteArtifacts(new[] { GetEntityArtifact(e.ShippingMethod) }));
            EventHub.NotificationEvents.OnShippingMethodDeleted((e) => _diskEntityService.DeleteArtifacts(new[] { GetEntityArtifact(e.ShippingMethod) }));

            // PaymentMethod
            EventHub.NotificationEvents.OnPaymentMethodSaved((e) => _diskEntityService.WriteArtifacts(new[] { GetEntityArtifact(e.PaymentMethod) }));
            EventHub.NotificationEvents.OnPaymentMethodDeleted((e) => _diskEntityService.DeleteArtifacts(new[] { GetEntityArtifact(e.PaymentMethod) }));

            // Country
            EventHub.NotificationEvents.OnCountrySaved((e) => _diskEntityService.WriteArtifacts(new[] { GetEntityArtifact(e.Country) }));
            EventHub.NotificationEvents.OnCountryDeleted((e) => _diskEntityService.DeleteArtifacts(new[] { GetEntityArtifact(e.Country) }));

            // Region
            EventHub.NotificationEvents.OnRegionSaved((e) => _diskEntityService.WriteArtifacts(new[] { GetEntityArtifact(e.Region) }));
            EventHub.NotificationEvents.OnRegionDeleted((e) => _diskEntityService.DeleteArtifacts(new[] { GetEntityArtifact(e.Region) }));

            // Currency
            EventHub.NotificationEvents.OnCurrencySaved((e) => _diskEntityService.WriteArtifacts(new[] { GetEntityArtifact(e.Currency) }));
            EventHub.NotificationEvents.OnCurrencyDeleted((e) => _diskEntityService.DeleteArtifacts(new[] { GetEntityArtifact(e.Currency) }));

            // TaxClass
            EventHub.NotificationEvents.OnTaxClassSaved((e) => _diskEntityService.WriteArtifacts(new[] { GetEntityArtifact(e.TaxClass) }));
            EventHub.NotificationEvents.OnTaxClassDeleted((e) => _diskEntityService.DeleteArtifacts(new[] { GetEntityArtifact(e.TaxClass) }));

            // EmailTemplate
            EventHub.NotificationEvents.OnEmailTemplateSaved((e) => _diskEntityService.WriteArtifacts(new[] { GetEntityArtifact(e.EmailTemplate) }));
            EventHub.NotificationEvents.OnEmailTemplateDeleted((e) => _diskEntityService.DeleteArtifacts(new[] { GetEntityArtifact(e.EmailTemplate) }));

            // PrintTemplate
            EventHub.NotificationEvents.OnPrintTemplateSaved((e) => _diskEntityService.WriteArtifacts(new[] { GetEntityArtifact(e.PrintTemplate) }));
            EventHub.NotificationEvents.OnPrintTemplateDeleted((e) => _diskEntityService.DeleteArtifacts(new[] { GetEntityArtifact(e.PrintTemplate) }));

            // ExportTemplate
            EventHub.NotificationEvents.OnExportTemplateSaved((e) => _diskEntityService.WriteArtifacts(new[] { GetEntityArtifact(e.ExportTemplate) }));
            EventHub.NotificationEvents.OnExportTemplateDeleted((e) => _diskEntityService.DeleteArtifacts(new[] { GetEntityArtifact(e.ExportTemplate) }));

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
