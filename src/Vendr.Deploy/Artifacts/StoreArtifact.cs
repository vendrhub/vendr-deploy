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

        public GuidUdi BaseCurrencyUdi { get; set; }

        public GuidUdi DefaultCountryUdi { get; set; }

        public GuidUdi DefaultTaxClassUdi { get; set; }

        public GuidUdi DefaultOrderStatusUdi { get; set; }

        public GuidUdi ErrorOrderStatusUdi { get; set; }

        public bool PricesIncludeTax { get; set; }

        public TimeSpan? CookieTimeout { get; set; }

        public string CartNumberTemplate { get; set; }

        public string OrderNumberTemplate { get; set; }

        public IEnumerable<string> ProductPropertyAliases { get; set; }

        public IEnumerable<string> ProductUniquenessPropertyAliases { get; set; }

        public GuidUdi ShareStockFromStoreUdi { get; set; }

        public int GiftCardCodeLength { get; set; }

        public int GiftCardDaysValid { get; set; }

        public string GiftCardCodeTemplate { get; set; }

        public IEnumerable<string> GiftCardPropertyAliases { get; set; }

        public int GiftCardActivationMethod  { get; set; }

        public GuidUdi GiftCardActivationOrderStatusUdi { get; set; }

        public GuidUdi DefaultGiftCardEmailTemplateUdi { get; set; }

        public GuidUdi ConfirmationEmailTemplateUdi { get; set; }
        public GuidUdi ErrorEmailTemplateUdi { get; set; }

        public string OrderEditorConfig { get; set; }

        public IEnumerable<string> AllowedUsers { get; set; }

        public IEnumerable<string> AllowedUserRoles { get; set; }

        public int SortOrder { get; set; }
    }
}
