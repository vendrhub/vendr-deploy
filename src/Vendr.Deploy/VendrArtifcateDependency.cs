using Umbraco.Core;
using Umbraco.Core.Deploy;

namespace Vendr.Deploy
{
    public class VendrArtifcateDependency : ArtifactDependency
    {
        public VendrArtifcateDependency(Udi udi, ArtifactDependencyMode mode = ArtifactDependencyMode.Match) 
            : base(udi, false, mode)
        { }
    }
}
