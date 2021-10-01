using System.Collections.Generic;

#if NETFRAMEWORK
using Umbraco.Core;
using Umbraco.Core.Deploy;
using Umbraco.Deploy.Artifacts;
#else
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Deploy.Infrastructure.Artifacts;
#endif

namespace Vendr.Deploy.Artifacts
{
    public abstract class StoreEntityArtifactBase : DeployArtifactBase<GuidUdi>
    {
        protected StoreEntityArtifactBase(GuidUdi udi, GuidUdi storeUdi, IEnumerable<ArtifactDependency> dependencies = null)
            : base(udi, dependencies)
        {
            StoreUdi = storeUdi;
        }

        public GuidUdi StoreUdi { get; set; }
    }
}
