using System.Collections.Generic;

#if NETFRAMEWORK
using Umbraco.Core;
using Umbraco.Core.Deploy;
#else
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
#endif

namespace Vendr.Deploy.Artifacts
{
    public class PrintTemplateArtifact : StoreEntityArtifactBase
    {
        public PrintTemplateArtifact(GuidUdi udi, GuidUdi storeUdi, IEnumerable<ArtifactDependency> dependencies = null)
            : base(udi, storeUdi, dependencies)
        { }

        public int Category { get; set; }

        public string TemplateView { get; set; }

        public int SortOrder { get; set; }
    }
}
