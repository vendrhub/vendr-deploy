using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Deploy;

namespace Vendr.Deploy.Artifacts
{
    public class RegionArtifact : StoreEntityArtifactBase
    {
        public RegionArtifact(GuidUdi udi, GuidUdi storeUdi, GuidUdi countryId, IEnumerable<ArtifactDependency> dependencies = null)
            : base(udi, storeUdi, dependencies)
        {
            CountryId = countryId;
        }

        public string Code { get; set; }

        public new string Alias
        {
            get => Code;
        }

        public GuidUdi CountryId { get; set; }
        public GuidUdi DefaultPaymentMethodId { get; set; }
        public GuidUdi DefaultShippingMethodId { get; set; }
        public int SortOrder { get; set; }
    }
}
