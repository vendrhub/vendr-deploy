using Newtonsoft.Json;
using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Deploy;
using Vendr.Deploy.Converters;

namespace Vendr.Deploy.Artifacts
{
    public class TaxClassArtifact : StoreEntityArtifactBase
    {
        public TaxClassArtifact(GuidUdi udi, GuidUdi storeUdi, IEnumerable<ArtifactDependency> dependencies = null)
            : base(udi, storeUdi, dependencies)
        { }

        [JsonConverter(typeof(RoundingDecimalJsonConverter), 3)]
        public decimal DefaultTaxRate { get; set; }

        public IEnumerable<CountryRegionTaxRateArtifact> CountryRegionTaxRates { get; set; }

        public int SortOrder { get; set; }
    }

    public class CountryRegionTaxRateArtifact
    {
        public GuidUdi CountryUdi { get; set; }

        public GuidUdi RegionUdi { get; set; }

        [JsonConverter(typeof(RoundingDecimalJsonConverter), 3)]
        public decimal TaxRate { get; set; }
    }
}
