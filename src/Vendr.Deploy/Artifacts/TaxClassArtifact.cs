using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Deploy;

namespace Vendr.Deploy.Artifacts
{
    public class TaxClassArtifact : StoreEntityArtifactBase
    {
        public TaxClassArtifact(GuidUdi udi, GuidUdi storeUdi, IEnumerable<ArtifactDependency> dependencies = null)
            : base(udi, storeUdi, dependencies)
        { }

        public decimal DefaultTaxRate { get; set; }

        public IEnumerable<CountryRegionTaxRateArtifact> CountryRegionTaxRates { get; set; }

        public int SortOrder { get; set; }
    }

    public class CountryRegionTaxRateArtifact
    {
        public GuidUdi CountryId { get; set; }

        public GuidUdi RegionId { get; set; }

        public decimal TaxRate { get; set; }
    }
}
