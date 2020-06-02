using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Deploy;

namespace Vendr.Deploy.Artifacts
{
    public class CountryArtifact : StoreEntityArtifactBase
    {
        public CountryArtifact(GuidUdi udi, GuidUdi storeUdi, IEnumerable<ArtifactDependency> dependencies = null)
            : base(udi, storeUdi, dependencies)
        { }

        public string Code { get; set; }

        public new string Alias
        {
            get => Code;
        }

        public GuidUdi DefaultCurrencyId { get; set; }
        public GuidUdi DefaultPaymentMethodId { get; set; }
        public GuidUdi DefaultShippingMethodId { get; set; }
        public int SortOrder { get; set; }
    }
}
