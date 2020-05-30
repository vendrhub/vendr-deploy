using System;
using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Deploy;
using Umbraco.Core.Services;
using Vendr.Core.Api;
using Vendr.Core.Models;
using Vendr.Deploy.Artifacts;

namespace Vendr.Deploy.Connectors.ServiceConnectors
{
    [UdiDefinition(Constants.UdiEntityType.Store, UdiType.GuidUdi)]
    public class VendrStoreServiceConnector : VendrEntityServiceConnectorBase<StoreArtifact, StoreReadOnly>
    {
        private readonly IUserService _userService;

        public override int[] ProcessPasses => new [] 
        {
            1,
            3
        };

        public override string[] ValidOpenSelectors => new []
        {
            "this-and-descendants",
            "descendants"
        };

        public override string AllEntitiesRangeName => "ALL STORED";

        public override string UdiEntityType => Constants.UdiEntityType.Store;

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
                var depUdi = new GuidUdi(Constants.UdiEntityType.Country, entity.DefaultCountryId.Value);
                var dep = new ArtifactDependency(depUdi, false, ArtifactDependencyMode.Exist);
                dependencies.Add(dep);
                artifact.DefaultCountryId = depUdi;
            }

            // Default tax class
            if (entity.DefaultTaxClassId.HasValue)
            {
                var depUdi = new GuidUdi(Constants.UdiEntityType.TaxClass, entity.DefaultTaxClassId.Value);
                var dep = new ArtifactDependency(depUdi, false, ArtifactDependencyMode.Exist);
                dependencies.Add(dep);
                artifact.DefaultTaxClassId = depUdi;
            }

            // Default order status
            if (entity.DefaultOrderStatusId.HasValue)
            {
                var depUdi = new GuidUdi(Constants.UdiEntityType.OrderStatus, entity.DefaultOrderStatusId.Value);
                var dep = new ArtifactDependency(depUdi, false, ArtifactDependencyMode.Exist);
                dependencies.Add(dep);
                artifact.DefaultOrderStatusId = depUdi;
            }

            // Error order status
            if (entity.ErrorOrderStatusId.HasValue)
            {
                var depUdi = new GuidUdi(Constants.UdiEntityType.OrderStatus, entity.ErrorOrderStatusId.Value);
                var dep = new ArtifactDependency(depUdi, false, ArtifactDependencyMode.Exist);
                dependencies.Add(dep);
                artifact.ErrorOrderStatusId = depUdi;
            }

            // Gift card activation order status
            if (entity.GiftCardActivationOrderStatusId.HasValue)
            {
                var depUdi = new GuidUdi(Constants.UdiEntityType.OrderStatus, entity.GiftCardActivationOrderStatusId.Value);
                var dep = new ArtifactDependency(depUdi, false, ArtifactDependencyMode.Exist);
                dependencies.Add(dep);
                artifact.GiftCardActivationOrderStatusId = depUdi;
            }

            // Gift card email template
            if (entity.DefaultGiftCardEmailTemplateId.HasValue)
            {
                var depUdi = new GuidUdi(Constants.UdiEntityType.EmailTemplate, entity.DefaultGiftCardEmailTemplateId.Value);
                var dep = new ArtifactDependency(depUdi, false, ArtifactDependencyMode.Exist);
                dependencies.Add(dep);
                artifact.DefaultGiftCardEmailTemplateId = depUdi;
            }

            // Confirmation email template
            if (entity.ConfirmationEmailTemplateId.HasValue)
            {
                var depUdi = new GuidUdi(Constants.UdiEntityType.EmailTemplate, entity.ConfirmationEmailTemplateId.Value);
                var dep = new ArtifactDependency(depUdi, false, ArtifactDependencyMode.Exist);
                dependencies.Add(dep);
                artifact.ConfirmationEmailTemplateId = depUdi;
            }

            // Error email template
            if (entity.ErrorEmailTemplateId.HasValue)
            {
                var depUdi = new GuidUdi(Constants.UdiEntityType.EmailTemplate, entity.ErrorEmailTemplateId.Value);
                var dep = new ArtifactDependency(depUdi, false, ArtifactDependencyMode.Exist);
                dependencies.Add(dep);
                artifact.ErrorEmailTemplateId = depUdi;
            }

            // Allowed users
            // NB: Users can't be deployed so don't add to dependencies collection
            // instead we'll just have to hope that they exist on all environments
            // and if not, when it comes to restore, we'll just not set anything
            if (entity.AllowedUsers.Count > 0)
            {
                var userUdis = new List<StringUdi>();

                foreach (var id in entity.AllowedUsers)
                {
                    var user = _userService.GetByProviderKey(id.UserId);
                    if (user != null)
                    {
                        userUdis.Add(new StringUdi("user", user.Username));
                    } 
                }

                if (userUdis.Count > 0)
                {
                    artifact.AllowedUsers = userUdis;
                }
            }

            // Allowed user roles
            // NB: Users roles can't be deployed so don't add to dependencies collection
            // instead we'll just have to hope that they exist on all environments
            // and if not, when it comes to restore, we'll just not set anything
            if (entity.AllowedUserRoles.Count > 0)
            {
                var userRoleUdis = new List<GuidUdi>();

                foreach (var role in entity.AllowedUserRoles)
                {
                    var userGroup = _userService.GetUserGroupByAlias(role.Role);
                    if (userGroup != null)
                    {
                        userRoleUdis.Add(new GuidUdi("user-group", userGroup.Key));
                    }
                }

                if (userRoleUdis.Count > 0)
                {
                    artifact.AllowedUserRoles = userRoleUdis;
                }
            }

            // Stock sharing store
            if (entity.ShareStockFromStoreId.HasValue)
            {
                var depUdi = new GuidUdi(Constants.UdiEntityType.Store, entity.ShareStockFromStoreId.Value);
                var dep = new ArtifactDependency(depUdi, true, ArtifactDependencyMode.Exist);
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
                default:
                    throw new ArgumentOutOfRangeException(nameof(pass));
            }
        }

        private void Pass1(ArtifactDeployState<StoreArtifact, StoreReadOnly> state, IDeployContext context)
        {
            using (var uow = _vendrApi.Uow.Create())
            {
                var artifact = state.Artifact;
                var entity = state.Entity?.AsWritable(uow) ?? Store.Create(uow, artifact.Udi.Guid, artifact.Alias, artifact.Name);
                
                // Default Order Status
                // Not sure if this needs to occur in a later pass

                Guid? defaultOrderStatusId = null;

                if (artifact.DefaultOrderStatusId != null)
                {
                    artifact.DefaultOrderStatusId.EnsureType(Constants.UdiEntityType.OrderStatus);

                    defaultOrderStatusId = _vendrApi.GetOrderStatus(artifact.DefaultOrderStatusId.Guid)?.Id;
                }

                entity.SetDefaultOrderStatus(defaultOrderStatusId);

                _vendrApi.SaveStore(entity);

                uow.Complete();
            }
        }
    }
}
