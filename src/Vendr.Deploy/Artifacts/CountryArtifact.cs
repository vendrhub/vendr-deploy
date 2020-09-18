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

        public GuidUdi DefaultCurrencyUdi { get; set; }
        public GuidUdi DefaultPaymentMethodUdi { get; set; }
        public GuidUdi DefaultShippingMethodUdi { get; set; }
        public int SortOrder { get; set; }
    }
}
