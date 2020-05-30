using System;
using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Deploy;
using Umbraco.Deploy.Artifacts;

namespace Vendr.Deploy.Artifacts
{
    public class StoreArtifact : DeployArtifactBase<GuidUdi>
    {
        public StoreArtifact(GuidUdi udi, IEnumerable<ArtifactDependency> dependencies = null) 
            : base(udi, dependencies)
        { }

        public GuidUdi DefaultCountryId { get; set; }

        public GuidUdi DefaultTaxClassId { get; set; }

        public GuidUdi DefaultOrderStatusId { get; set; }

        public GuidUdi ErrorOrderStatusId { get; set; }

        public bool PricesIncludeTax { get; set; }

        public TimeSpan? CookieTimeout { get; set; }

        public string CartNumberTemplate { get; set; }

        public string OrderNumberTemplate { get; set; }

        public IEnumerable<string> ProductPropertyAliases { get; set; }

        public IEnumerable<string> ProductUniquenessPropertyAliases { get; set; }

        public GuidUdi ShareStockFromStoreId { get; set; }

        public int GiftCardCodeLength { get; set; }

        public int GiftCardDaysValid { get; set; }

        public string GiftCardCodeTemplate { get; set; }

        public IEnumerable<string> GiftCardPropertyAliases { get; set; }

        public int GiftCardActivationMethod  { get; set; }

        public GuidUdi GiftCardActivationOrderStatusId { get; set; }

        public GuidUdi DefaultGiftCardEmailTemplateId { get; set; }

        public GuidUdi ConfirmationEmailTemplateId { get; set; }
        public GuidUdi ErrorEmailTemplateId { get; set; }

        public string OrderEditorConfig { get; set; }

        public IEnumerable<StringUdi> AllowedUsers { get; set; }

        public IEnumerable<GuidUdi> AllowedUserRoles { get; set; }

        public int SortOrder { get; set; }
    }
}
