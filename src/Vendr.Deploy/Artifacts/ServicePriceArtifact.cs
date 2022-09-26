using Newtonsoft.Json;
using Vendr.Deploy.Converters;
using Umbraco.Cms.Core;

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
