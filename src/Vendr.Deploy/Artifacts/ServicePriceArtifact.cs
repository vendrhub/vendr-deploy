using Newtonsoft.Json;
using Umbraco.Cms.Core;
using Umbraco.Deploy.Infrastructure.Serialization;

namespace Vendr.Deploy.Artifacts
{
    public class ServicePriceArtifact
    {
        public GuidUdi CurrencyUdi { get; set; }
        public GuidUdi CountryUdi { get; set; }
        public GuidUdi RegionUdi { get; set; }

        [JsonConverter(typeof(RoundingDecimalJsonConverter), 3)]
        public decimal Value { get; set; }
    }
}
