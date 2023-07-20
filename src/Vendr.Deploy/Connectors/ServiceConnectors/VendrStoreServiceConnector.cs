using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Vendr.Core.Api;
using Vendr.Core.Models;
using Vendr.Deploy.Artifacts;
using Vendr.Deploy.Configuration;

using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Cms.Core.Services;

namespace Vendr.Deploy.Connectors.ServiceConnectors
{
    [UdiDefinition(VendrConstants.UdiEntityType.Store, UdiType.GuidUdi)]
    public class VendrStoreServiceConnector : VendrEntityServiceConnectorBase<StoreArtifact, StoreReadOnly>
    {
        private readonly IUserService _userService;

        public override int[] ProcessPasses => new[]
        {
            1,4
        };

        public override string[] ValidOpenSelectors => new[]
        {
            "this",
            "this-and-descendants",
            "descendants"
        };

        public override string AllEntitiesRangeName => "All Vendr Stores";

        public override string UdiEntityType => VendrConstants.UdiEntityType.Store;

        public VendrStoreServiceConnector(IVendrApi vendrApi, VendrDeploySettingsAccessor settingsAccessor, IUserService userService)
            : base(vendrApi, settingsAccessor)
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

#pragma warning disable CS0618 // OrderEditorConfig is obsolete
            var artifact = new StoreArtifact(udi, dependencies)
            {
                Name = entity.Name,
                Alias = entity.Alias,
                PricesIncludeTax = entity.PricesIncludeTax,
                CookieTimeout = entity.CookieTimeout,
                CartNumberTemplate = entity.CartNumberTemplate,
                OrderNumberTemplate = entity.OrderNumberTemplate,
                OrderRoundingMethod = (int)entity.OrderRoundingMethod,
                ProductPropertyAliases = entity.ProductPropertyAliases,
                ProductUniquenessPropertyAliases = entity.ProductUniquenessPropertyAliases,
                GiftCardCodeLength = entity.GiftCardCodeLength,
                GiftCardDaysValid = entity.GiftCardDaysValid,
                GiftCardCodeTemplate = entity.GiftCardCodeTemplate,
                GiftCardPropertyAliases = entity.GiftCardPropertyAliases,
                GiftCardActivationMethod = (int)entity.GiftCardActivationMethod,
                OrderEditorConfig = entity.OrderEditorConfig
            };
#pragma warning restore CS0618 // OrderEditorConfig is obsolete

            // Base currency
            if (entity.BaseCurrencyId.HasValue)
            {
                var depUdi = new GuidUdi(VendrConstants.UdiEntityType.Currency, entity.BaseCurrencyId.Value);
                var dep = new VendrArtifactDependency(depUdi);
                dependencies.Add(dep);
                artifact.BaseCurrencyUdi = depUdi;
            }

            // Default country
            if (entity.DefaultCountryId.HasValue)
            {
                var depUdi = new GuidUdi(VendrConstants.UdiEntityType.Country, entity.DefaultCountryId.Value);
                var dep = new VendrArtifactDependency(depUdi);
                dependencies.Add(dep);
                artifact.DefaultCountryUdi = depUdi;
            }

            // Default tax class
            if (entity.DefaultTaxClassId.HasValue)
            {
                var depUdi = new GuidUdi(VendrConstants.UdiEntityType.TaxClass, entity.DefaultTaxClassId.Value);
                var dep = new VendrArtifactDependency(depUdi);
                dependencies.Add(dep);
                artifact.DefaultTaxClassUdi = depUdi;
            }

            // Default order status
            if (entity.DefaultOrderStatusId.HasValue)
            {
                var depUdi = new GuidUdi(VendrConstants.UdiEntityType.OrderStatus, entity.DefaultOrderStatusId.Value);
                var dep = new VendrArtifactDependency(depUdi);
                dependencies.Add(dep);
                artifact.DefaultOrderStatusUdi = depUdi;
            }

            // Error order status
            if (entity.ErrorOrderStatusId.HasValue)
            {
                var depUdi = new GuidUdi(VendrConstants.UdiEntityType.OrderStatus, entity.ErrorOrderStatusId.Value);
                var dep = new VendrArtifactDependency(depUdi);
                dependencies.Add(dep);
                artifact.ErrorOrderStatusUdi = depUdi;
            }

            // Gift card activation order status
            if (entity.GiftCardActivationOrderStatusId.HasValue)
            {
                var depUdi = new GuidUdi(VendrConstants.UdiEntityType.OrderStatus, entity.GiftCardActivationOrderStatusId.Value);
                var dep = new VendrArtifactDependency(depUdi);
                dependencies.Add(dep);
                artifact.GiftCardActivationOrderStatusUdi = depUdi;
            }

            // Gift card email template
            if (entity.DefaultGiftCardEmailTemplateId.HasValue)
            {
                var depUdi = new GuidUdi(VendrConstants.UdiEntityType.EmailTemplate, entity.DefaultGiftCardEmailTemplateId.Value);
                var dep = new VendrArtifactDependency(depUdi);
                dependencies.Add(dep);
                artifact.DefaultGiftCardEmailTemplateUdi = depUdi;
            }

            // Confirmation email template
            if (entity.ConfirmationEmailTemplateId.HasValue)
            {
                var depUdi = new GuidUdi(VendrConstants.UdiEntityType.EmailTemplate, entity.ConfirmationEmailTemplateId.Value);
                var dep = new VendrArtifactDependency(depUdi);
                dependencies.Add(dep);
                artifact.ConfirmationEmailTemplateUdi = depUdi;
            }

            // Error email template
            if (entity.ErrorEmailTemplateId.HasValue)
            {
                var depUdi = new GuidUdi(VendrConstants.UdiEntityType.EmailTemplate, entity.ErrorEmailTemplateId.Value);
                var dep = new VendrArtifactDependency(depUdi);
                dependencies.Add(dep);
                artifact.ErrorEmailTemplateUdi = depUdi;
            }

            // Allowed users
            // NB: Users can't be deployed so don't add to dependencies collection
            // instead we'll just have to hope that they exist on all environments
            // and if not, when it comes to restore, we'll just not set anything
            if (entity.AllowedUsers.Count > 0)
            {
                var users = new List<string>();

                foreach (var id in entity.AllowedUsers.OrderBy(x => x.UserId))
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

                foreach (var role in entity.AllowedUserRoles.OrderBy(x => x.Role))
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
                var dep = new VendrArtifactDependency(depUdi);
                dependencies.Add(dep);
                artifact.ShareStockFromStoreUdi = depUdi;
            }

            return artifact;
        }

        public override void Process(ArtifactDeployState<StoreArtifact, StoreReadOnly> state, IDeployContext context, int pass)
        {
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
            _vendrApi.Uow.Execute(uow =>
            {
                var artifact = state.Artifact;

                artifact.Udi.EnsureType(VendrConstants.UdiEntityType.Store);

                var entity = state.Entity?.AsWritable(uow)
                    ?? Store.Create(uow, artifact.Udi.Guid, artifact.Alias, artifact.Name, false);

#pragma warning disable CS0618 // SetOrderEditorConfig is obsolete
                entity.SetName(artifact.Name, artifact.Alias)
                    .SetPriceTaxInclusivity(artifact.PricesIncludeTax)
                    .SetCartNumberTemplate(artifact.CartNumberTemplate)
                    .SetOrderNumberTemplate(artifact.OrderNumberTemplate)
                    .SetOrderRoundingMethod((OrderRoundingMethod)artifact.OrderRoundingMethod)
                    .SetProductPropertyAliases(artifact.ProductPropertyAliases, SetBehavior.Replace)
                    .SetProductUniquenessPropertyAliases(artifact.ProductUniquenessPropertyAliases, SetBehavior.Replace)
                    .SetGiftCardCodeLength(artifact.GiftCardCodeLength)
                    .SetGiftCardCodeTemplate(artifact.GiftCardCodeTemplate)
                    .SetGiftCardValidityTimeframe(artifact.GiftCardDaysValid)
                    .SetGiftCardPropertyAliases(artifact.GiftCardPropertyAliases, SetBehavior.Replace)
                    .SetGiftCardActivationMethod((GiftCardActivationMethod)artifact.GiftCardActivationMethod)
                    .SetOrderEditorConfig(artifact.OrderEditorConfig)
                    .SetSortOrder(artifact.SortOrder);
#pragma warning restore CS0618 // SetOrderEditorConfig is obsolete

                if (artifact.CookieTimeout.HasValue)
                {
                    entity.EnableCookies(artifact.CookieTimeout.Value);
                }
                else
                {
                    entity.DisableCookies();
                }

                if (artifact.AllowedUsers != null && artifact.AllowedUsers.Any())
                {
                    var userIds = artifact.AllowedUsers.Select(x => _userService.GetByUsername(x))
                        .Where(x => x != null)
                        .Select(x => x.Id.ToString(CultureInfo.InvariantCulture))
                        .ToList();

                    entity.SetAllowedUsers(userIds, SetBehavior.Replace);
                }

                if (artifact.AllowedUserRoles != null && artifact.AllowedUserRoles.Any())
                {
                    var userRoles = artifact.AllowedUserRoles.Select(x => _userService.GetUserGroupByAlias(x))
                        .Where(x => x != null)
                        .Select(x => x.Alias)
                        .ToList();

                    entity.SetAllowedUserRoles(userRoles, SetBehavior.Replace);
                }

                _vendrApi.SaveStore(entity);

                state.Entity = entity;

                uow.Complete();
            });
        }

        private void Pass4(ArtifactDeployState<StoreArtifact, StoreReadOnly> state, IDeployContext context)
        {
            _vendrApi.Uow.Execute(uow =>
            {
                var artifact = state.Artifact;
                var entity = _vendrApi.GetStore(state.Entity.Id).AsWritable(uow);

                // BaseCurrency
                Guid? baseCurrencyId = null;

                if (artifact.BaseCurrencyUdi != null)
                {
                    artifact.BaseCurrencyUdi.EnsureType(VendrConstants.UdiEntityType.Currency);

                    baseCurrencyId = _vendrApi.GetCurrency(artifact.BaseCurrencyUdi.Guid)?.Id;
                }

                entity.SetBaseCurrency(baseCurrencyId);

                // DefaultCountry
                Guid? defaultCountryId = null;

                if (artifact.DefaultCountryUdi != null)
                {
                    artifact.DefaultCountryUdi.EnsureType(VendrConstants.UdiEntityType.Country);

                    defaultCountryId = _vendrApi.GetCountry(artifact.DefaultCountryUdi.Guid)?.Id;
                }

                entity.SetDefaultCountry(defaultCountryId);

                // DefaultTaxClass
                Guid? defaultTaxClassId = null;

                if (artifact.DefaultTaxClassUdi != null)
                {
                    artifact.DefaultTaxClassUdi.EnsureType(VendrConstants.UdiEntityType.TaxClass);

                    defaultTaxClassId = _vendrApi.GetTaxClass(artifact.DefaultTaxClassUdi.Guid)?.Id;
                }

                entity.SetDefaultTaxClass(defaultTaxClassId);

                // DefaultOrderStatus
                Guid? defaultOrderStatusId = null;

                if (artifact.DefaultOrderStatusUdi != null)
                {
                    artifact.DefaultOrderStatusUdi.EnsureType(VendrConstants.UdiEntityType.OrderStatus);

                    defaultOrderStatusId = _vendrApi.GetOrderStatus(artifact.DefaultOrderStatusUdi.Guid)?.Id;
                }

                entity.SetDefaultOrderStatus(defaultOrderStatusId);

                // ErrorOrderStatus
                Guid? errorOrderStatusId = null;

                if (artifact.ErrorOrderStatusUdi != null)
                {
                    artifact.ErrorOrderStatusUdi.EnsureType(VendrConstants.UdiEntityType.OrderStatus);

                    errorOrderStatusId = _vendrApi.GetOrderStatus(artifact.ErrorOrderStatusUdi.Guid)?.Id;
                }

                entity.SetErrorOrderStatus(errorOrderStatusId);

                // DefaultGiftCardEmailTemplate
                Guid? defaultGiftCardEmailTemplateId = null;

                if (artifact.DefaultGiftCardEmailTemplateUdi != null)
                {
                    artifact.DefaultGiftCardEmailTemplateUdi.EnsureType(VendrConstants.UdiEntityType.EmailTemplate);

                    defaultGiftCardEmailTemplateId = _vendrApi.GetEmailTemplate(artifact.DefaultGiftCardEmailTemplateUdi.Guid)?.Id;
                }

                entity.SetDefaultGiftCardEmailTemplate(defaultGiftCardEmailTemplateId);

                // ConfirmationEmailTemplate
                Guid? confirmationEmailTemplateId = null;

                if (artifact.ConfirmationEmailTemplateUdi != null)
                {
                    artifact.ConfirmationEmailTemplateUdi.EnsureType(VendrConstants.UdiEntityType.EmailTemplate);

                    confirmationEmailTemplateId = _vendrApi.GetEmailTemplate(artifact.ConfirmationEmailTemplateUdi.Guid)?.Id;
                }

                entity.SetConfirmationEmailTemplate(confirmationEmailTemplateId);

                // ErrorEmailTemplate
                Guid? errorEmailTemplateId = null;

                if (artifact.ErrorEmailTemplateUdi != null)
                {
                    artifact.ErrorEmailTemplateUdi.EnsureType(VendrConstants.UdiEntityType.EmailTemplate);

                    errorEmailTemplateId = _vendrApi.GetEmailTemplate(artifact.ErrorEmailTemplateUdi.Guid)?.Id;
                }

                entity.SetErrorEmailTemplate(errorEmailTemplateId);

                // StockSharingStore
                Guid? stockSharingStore = null;

                if (artifact.ShareStockFromStoreUdi != null)
                {
                    artifact.ShareStockFromStoreUdi.EnsureType(VendrConstants.UdiEntityType.Store);

                    stockSharingStore = _vendrApi.GetStore(artifact.ShareStockFromStoreUdi.Guid)?.Id;
                }

                if (stockSharingStore.HasValue)
                    entity.ShareStockFrom(stockSharingStore.Value);
                else
                    entity.StopSharingStock();

                _vendrApi.SaveStore(entity);

                uow.Complete();
            });
        }
    }
}
