using System.Collections.Generic;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;

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
