using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Deploy;
using Umbraco.Deploy.Artifacts;

namespace Vendr.Deploy.Artifacts
{
    public abstract class StoreEntityArtifactBase : DeployArtifactBase<GuidUdi>
    {
        public StoreEntityArtifactBase(GuidUdi udi, GuidUdi storeUdi, IEnumerable<ArtifactDependency> dependencies = null)
            : base(udi, dependencies)
        {
            StoreId = storeUdi;
        }

        public GuidUdi StoreId { get; set; }
    }
}
