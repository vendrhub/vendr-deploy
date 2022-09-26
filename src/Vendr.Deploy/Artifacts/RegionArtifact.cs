using System.Collections.Generic;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;

namespace Vendr.Deploy.Artifacts
{
    public class RegionArtifact : StoreEntityArtifactBase
    {
        public RegionArtifact(GuidUdi udi, GuidUdi storeUdi, GuidUdi countryUdi, IEnumerable<ArtifactDependency> dependencies = null)
            : base(udi, storeUdi, dependencies)
        {
            CountryUdi = countryUdi;
        }

        public string Code { get; set; }

        public new string Alias
        {
            get => Code;
        }

        public GuidUdi CountryUdi { get; set; }
        public GuidUdi DefaultPaymentMethodUdi { get; set; }
        public GuidUdi DefaultShippingMethodUdi { get; set; }
        public int SortOrder { get; set; }
    }
}
