using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Deploy;
using Umbraco.Core.Services;
using Vendr.Core.Api;
using Vendr.Core.Models;
using Vendr.Deploy.Artifacts;

namespace Vendr.Deploy.Connectors.ServiceConnectors
{
    [UdiDefinition(VendrConstants.UdiEntityType.Store, UdiType.GuidUdi)]
    public class VendrStoreServiceConnector : VendrEntityServiceConnectorBase<StoreArtifact, StoreReadOnly>
    {
        private readonly IUserService _userService;

        public override int[] ProcessPasses => new [] 
        {
            1,4
        };

        public override string[] ValidOpenSelectors => new []
        {
            "this-and-descendants",
            "descendants"
        };

        public override string AllEntitiesRangeName => "ALL STORED";

        public override string UdiEntityType => VendrConstants.UdiEntityType.Store;

        public VendrStoreServiceConnector(IVendrApi vendrApi, IUserService userService)
            : base(vendrApi)
        {
            _userService = userService;
        }

        public override string GetEntityName(StoreReadOnly entity)
            => entity.Name;

        public override StoreReadOnly GetEntity(Guid id)
            => _vendrApi.GetStore(id);

        public override IEnumerable<StoreReadOnly> GetEntities()
            => _vendrApi.GetStores();

        public override StoreArtifact GetArtifact(GuidUdi udi, StoreReadOnly entity)
        {
            if (entity == null)
                return null;

            // TODO: Add the "defaults" as dependencies?

            var dependencies = new ArtifactDependencyCollection();

            var artifact = new StoreArtifact(udi, dependencies)
            {
                Name = entity.Name,
                Alias = entity.Alias,
                PricesIncludeTax = entity.PricesIncludeTax,
                CookieTimeout = entity.CookieTimeout,
                CartNumberTemplate = entity.CartNumberTemplate,
                OrderNumberTemplate = entity.OrderNumberTemplate,
                ProductPropertyAliases = entity.ProductPropertyAliases,
                ProductUniquenessPropertyAliases = entity.ProductUniquenessPropertyAliases,
                GiftCardCodeLength = entity.GiftCardCodeLength,
                GiftCardDaysValid = entity.GiftCardDaysValid,
                GiftCardCodeTemplate = entity.GiftCardCodeTemplate,
                GiftCardPropertyAliases = entity.GiftCardPropertyAliases,
                GiftCardActivationMethod = (int)entity.GiftCardActivationMethod,
                OrderEditorConfig = entity.OrderEditorConfig
            };

            // Default country
            if (entity.DefaultCountryId.HasValue)
            {
                var depUdi = new GuidUdi(VendrConstants.UdiEntityType.Country, entity.DefaultCountryId.Value);
                var dep = new VendrArtifcateDependency(depUdi);
                dependencies.Add(dep);
                artifact.DefaultCountryId = depUdi;
            }

            // Default tax class
            if (entity.DefaultTaxClassId.HasValue)
            {
                var depUdi = new GuidUdi(VendrConstants.UdiEntityType.TaxClass, entity.DefaultTaxClassId.Value);
                var dep = new VendrArtifcateDependency(depUdi);
                dependencies.Add(dep);
                artifact.DefaultTaxClassId = depUdi;
            }

            // Default order status
            if (entity.DefaultOrderStatusId.HasValue)
            {
                var depUdi = new GuidUdi(VendrConstants.UdiEntityType.OrderStatus, entity.DefaultOrderStatusId.Value);
                var dep = new VendrArtifcateDependency(depUdi);
                dependencies.Add(dep);
                artifact.DefaultOrderStatusId = depUdi;
            }

            // Error order status
            if (entity.ErrorOrderStatusId.HasValue)
            {
                var depUdi = new GuidUdi(VendrConstants.UdiEntityType.OrderStatus, entity.ErrorOrderStatusId.Value);
                var dep = new VendrArtifcateDependency(depUdi);
                dependencies.Add(dep);
                artifact.ErrorOrderStatusId = depUdi;
            }

            // Gift card activation order status
            if (entity.GiftCardActivationOrderStatusId.HasValue)
            {
                var depUdi = new GuidUdi(VendrConstants.UdiEntityType.OrderStatus, entity.GiftCardActivationOrderStatusId.Value);
                var dep = new VendrArtifcateDependency(depUdi);
                dependencies.Add(dep);
                artifact.GiftCardActivationOrderStatusId = depUdi;
            }

            // Gift card email template
            if (entity.DefaultGiftCardEmailTemplateId.HasValue)
            {
                var depUdi = new GuidUdi(VendrConstants.UdiEntityType.EmailTemplate, entity.DefaultGiftCardEmailTemplateId.Value);
                var dep = new VendrArtifcateDependency(depUdi);
                dependencies.Add(dep);
                artifact.DefaultGiftCardEmailTemplateId = depUdi;
            }

            // Confirmation email template
            if (entity.ConfirmationEmailTemplateId.HasValue)
            {
                var depUdi = new GuidUdi(VendrConstants.UdiEntityType.EmailTemplate, entity.ConfirmationEmailTemplateId.Value);
                var dep = new VendrArtifcateDependency(depUdi);
                dependencies.Add(dep);
                artifact.ConfirmationEmailTemplateId = depUdi;
            }

            // Error email template
            if (entity.ErrorEmailTemplateId.HasValue)
            {
                var depUdi = new GuidUdi(VendrConstants.UdiEntityType.EmailTemplate, entity.ErrorEmailTemplateId.Value);
                var dep = new VendrArtifcateDependency(depUdi);
                dependencies.Add(dep);
                artifact.ErrorEmailTemplateId = depUdi;
            }

            // Allowed users
            // NB: Users can't be deployed so don't add to dependencies collection
            // instead we'll just have to hope that they exist on all environments
            // and if not, when it comes to restore, we'll just not set anything
            if (entity.AllowedUsers.Count > 0)
            {
                var users = new List<string>();

                foreach (var id in entity.AllowedUsers)
                {
                    var user = _userService.GetByProviderKey(id.UserId);
                    if (user != null)
                    {
                        users.Add(user.Username);
                    }
                }

                if (users.Count > 0)
                {
                    artifact.AllowedUsers = users;
                }
            }

            // Allowed user roles
            // NB: Users roles can't be deployed so don't add to dependencies collection
            // instead we'll just have to hope that they exist on all environments
            // and if not, when it comes to restore, we'll just not set anything
            if (entity.AllowedUserRoles.Count > 0)
            {
                var userRoles = new List<string>();

                foreach (var role in entity.AllowedUserRoles)
                {
                    var userGroup = _userService.GetUserGroupByAlias(role.Role);
                    if (userGroup != null)
                    {
                        userRoles.Add(userGroup.Alias);
                    }
                }

                if (userRoles.Count > 0)
                {
                    artifact.AllowedUserRoles = userRoles;
                }
            }

            // Stock sharing store
            if (entity.ShareStockFromStoreId.HasValue)
            {
                var depUdi = new GuidUdi(VendrConstants.UdiEntityType.Store, entity.ShareStockFromStoreId.Value);
                var dep = new VendrArtifcateDependency(depUdi);
                dependencies.Add(dep);
                artifact.ShareStockFromStoreId = depUdi;
            }

            return artifact;
        }
                
        public override void Process(ArtifactDeployState<StoreArtifact, StoreReadOnly> state, IDeployContext context, int pass)
        {
            // TODO: NEED TO DO MULTI PASSES FOR INNER ENTITIES

            state.NextPass = GetNextPass(ProcessPasses, pass);

            switch (pass)
            {
                case 1:
                    Pass1(state, context);
                    break;
                case 4:
                    Pass4(state, context);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(pass));
            }
        }

        private void Pass1(ArtifactDeployState<StoreArtifact, StoreReadOnly> state, IDeployContext context)
        {
            using (var uow = _vendrApi.Uow.Create())
            {
                var artifact = state.Artifact;

                artifact.Udi.EnsureType(VendrConstants.UdiEntityType.Store);

                var entity = state.Entity?.AsWritable(uow) 
                    ?? Store.Create(uow, artifact.Udi.Guid, artifact.Alias, artifact.Name, false);

                entity.SetName(artifact.Name, artifact.Alias)
                    .SetPriceTaxInclusivity(artifact.PricesIncludeTax)
                    .SetCartNumberTemplate(artifact.CartNumberTemplate)
                    .SetOrderNumberTemplate(artifact.OrderNumberTemplate)
                    .SetProductPropertyAliases(artifact.ProductPropertyAliases, SetBehavior.Merge)
                    .SetProductUniquenessPropertyAliases(artifact.ProductUniquenessPropertyAliases, SetBehavior.Merge)
                    .SetGiftCardCodeLength(artifact.GiftCardCodeLength)
                    .SetGiftCardValidityTimeframe(artifact.GiftCardDaysValid)
                    .SetGiftCardPropertyAliases(artifact.GiftCardPropertyAliases, SetBehavior.Merge)
                    .SetGiftCardActivationMethod((GiftCardActivationMethod)artifact.GiftCardActivationMethod)
                    .SetOrderEditorConfig(artifact.OrderEditorConfig)
                    .SetSortOrder(artifact.SortOrder);
    
                if (artifact.CookieTimeout.HasValue)
                {
                    entity.EnableCookies(artifact.CookieTimeout.Value);
                }
                else
                {
                    entity.DisableCookies();
                }

                var userIds = artifact.AllowedUsers.Select(x => _userService.GetByUsername(x))
                    .Where(x => x != null)
                    .Select(x => x.Id.ToString(CultureInfo.InvariantCulture))
                    .ToList();

                entity.SetAllowedUsers(userIds, SetBehavior.Merge);

                var userRoles = artifact.AllowedUserRoles.Select(x => _userService.GetUserGroupByAlias(x))
                    .Where(x => x != null)
                    .Select(x => x.Alias)
                    .ToList();

                entity.SetAllowedUserRoles(userRoles, SetBehavior.Merge);

                _vendrApi.SaveStore(entity);

                state.Entity = entity;

                uow.Complete();
            }
        }

        private void Pass4(ArtifactDeployState<StoreArtifact, StoreReadOnly> state, IDeployContext context)
        {
            using (var uow = _vendrApi.Uow.Create())
            {
                var artifact = state.Artifact;
                var entity = state.Entity.AsWritable(uow);

                // DefaultCountry
                Guid? defaultCountryId = null;

                if (artifact.DefaultCountryId != null)
                {
                    artifact.DefaultCountryId.EnsureType(VendrConstants.UdiEntityType.Country);

                    defaultCountryId = _vendrApi.GetCountry(artifact.DefaultCountryId.Guid)?.Id;
                }

                entity.SetDefaultCountry(defaultCountryId);

                // DefaultTaxClass
                Guid? defaultTaxClassId = null;

                if (artifact.DefaultTaxClassId != null)
                {
                    artifact.DefaultTaxClassId.EnsureType(VendrConstants.UdiEntityType.TaxClass);

                    defaultTaxClassId = _vendrApi.GetTaxClass(artifact.DefaultTaxClassId.Guid)?.Id;
                }

                entity.SetDefaultTaxClass(defaultTaxClassId);

                // DefaultOrderStatus
                Guid? defaultOrderStatusId = null;

                if (artifact.DefaultOrderStatusId != null)
                {
                    artifact.DefaultOrderStatusId.EnsureType(VendrConstants.UdiEntityType.OrderStatus);

                    defaultOrderStatusId = _vendrApi.GetOrderStatus(artifact.DefaultOrderStatusId.Guid)?.Id;
                }

                entity.SetDefaultOrderStatus(defaultOrderStatusId);

                // ErrorOrderStatus
                Guid? errorOrderStatusId = null;

                if (artifact.ErrorOrderStatusId != null)
                {
                    artifact.ErrorOrderStatusId.EnsureType(VendrConstants.UdiEntityType.OrderStatus);

                    errorOrderStatusId = _vendrApi.GetOrderStatus(artifact.ErrorOrderStatusId.Guid)?.Id;
                }

                entity.SetErrorOrderStatus(errorOrderStatusId);

                // DefaultGiftCardEmailTemplate
                Guid? defaultGiftCardEmailTemplateId = null;

                if (artifact.DefaultGiftCardEmailTemplateId != null)
                {
                    artifact.DefaultGiftCardEmailTemplateId.EnsureType(VendrConstants.UdiEntityType.EmailTemplate);

                    defaultGiftCardEmailTemplateId = _vendrApi.GetEmailTemplate(artifact.DefaultGiftCardEmailTemplateId.Guid)?.Id;
                }

                entity.SetDefaultGiftCardEmailTemplate(defaultGiftCardEmailTemplateId);

                // ConfirmationEmailTemplate
                Guid? confirmationEmailTemplateId = null;

                if (artifact.ConfirmationEmailTemplateId != null)
                {
                    artifact.ConfirmationEmailTemplateId.EnsureType(VendrConstants.UdiEntityType.EmailTemplate);

                    confirmationEmailTemplateId = _vendrApi.GetEmailTemplate(artifact.ConfirmationEmailTemplateId.Guid)?.Id;
                }

                entity.SetConfirmationEmailTemplate(confirmationEmailTemplateId);

                // ErrorEmailTemplate
                Guid? errorEmailTemplateId = null;

                if (artifact.ErrorEmailTemplateId != null)
                {
                    artifact.ErrorEmailTemplateId.EnsureType(VendrConstants.UdiEntityType.EmailTemplate);

                    errorEmailTemplateId = _vendrApi.GetEmailTemplate(artifact.ErrorEmailTemplateId.Guid)?.Id;
                }

                entity.SetErrorEmailTemplate(errorEmailTemplateId);

                // StockSharingStore
                Guid? stockSharingStore = null;

                if (artifact.ShareStockFromStoreId != null)
                {
                    artifact.ShareStockFromStoreId.EnsureType(VendrConstants.UdiEntityType.Store);

                    stockSharingStore = _vendrApi.GetStore(artifact.ShareStockFromStoreId.Guid)?.Id;
                }

                if (stockSharingStore.HasValue)
                    entity.ShareStockFrom(stockSharingStore.Value);
                else
                    entity.StopSharingStock();

                _vendrApi.SaveStore(entity);

                uow.Complete();
            }
        }
    }
}
