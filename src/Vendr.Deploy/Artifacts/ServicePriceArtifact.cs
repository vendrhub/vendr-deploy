using Umbraco.Core;

namespace Vendr.Deploy.Artifacts
{
    public class ServicePriceArtifact
    {
        public GuidUdi CurrencyUdi { get; set; }
        public GuidUdi CountryUdi { get; set; }
        public GuidUdi RegionUdi { get; set; }
        public decimal Value { get; set; }
    }
}
