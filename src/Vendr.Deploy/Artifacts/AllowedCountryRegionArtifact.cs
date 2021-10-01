#if NETFRAMEWORK
using Umbraco.Core;
#else
using Umbraco.Cms.Core;
#endif

namespace Vendr.Deploy.Artifacts
{
    public class AllowedCountryRegionArtifact : AllowedCountryArtifact
    {
        public GuidUdi RegionUdi { get; set; }
    }
}
