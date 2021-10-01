#if NETFRAMEWORK
using Umbraco.Core;
using Umbraco.Core.Deploy;
#else
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
#endif

namespace Vendr.Deploy
{
    public class VendrArtifactDependency : ArtifactDependency
    {
        public VendrArtifactDependency(Udi udi, ArtifactDependencyMode mode = ArtifactDependencyMode.Match)
            : base(udi, false, mode)
        { }
    }
}
