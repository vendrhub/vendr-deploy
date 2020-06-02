using Umbraco.Core;

namespace Vendr.Deploy.Artifacts
{
    public class ServicePriceArtifact
    {
        public GuidUdi CurrencyId { get; set; }
        public GuidUdi CountryId { get; set; }
        public GuidUdi RegionId { get; set; }
        public decimal Value { get; set; }
    }
}
