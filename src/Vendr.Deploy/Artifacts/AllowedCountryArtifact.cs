#if NETFRAMEWORK
using Umbraco.Core;
#else
using Umbraco.Cms.Core;
#endif

namespace Vendr.Deploy.Artifacts
{
    public class AllowedCountryArtifact
    {
        public GuidUdi CountryUdi { get; set; }
    }
}
