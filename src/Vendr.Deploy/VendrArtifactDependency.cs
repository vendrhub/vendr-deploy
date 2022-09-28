using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;

namespace Vendr.Deploy
{
    public class VendrArtifactDependency : ArtifactDependency
    {
        public VendrArtifactDependency(Udi udi, ArtifactDependencyMode mode = ArtifactDependencyMode.Exist)
            : base(udi, false, mode)
        { }
    }
}
