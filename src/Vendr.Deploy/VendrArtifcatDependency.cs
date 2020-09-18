using Umbraco.Core;
using Umbraco.Core.Deploy;

namespace Vendr.Deploy
{
    public class VendrArtifcatDependency : ArtifactDependency
    {
        public VendrArtifcatDependency(Udi udi, ArtifactDependencyMode mode = ArtifactDependencyMode.Match) 
            : base(udi, false, mode)
        { }
    }
}
