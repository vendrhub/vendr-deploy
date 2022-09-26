using System.Collections.Generic;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;

namespace Vendr.Deploy.Artifacts
{
    public class ShippingMethodArtifact : StoreEntityArtifactBase
    {
        public ShippingMethodArtifact(GuidUdi udi, GuidUdi storeUdi, IEnumerable<ArtifactDependency> dependencies = null)
            : base(udi, storeUdi, dependencies)
        { }

        public string Sku { get; set; }
        public GuidUdi TaxClassUdi { get; set; }
        public IEnumerable<ServicePriceArtifact> Prices { get; set; }
        public string ImageId { get; set; }
        public IEnumerable<AllowedCountryRegionArtifact> AllowedCountryRegions { get; set; }
        public int SortOrder { get; set; }
    }
}
