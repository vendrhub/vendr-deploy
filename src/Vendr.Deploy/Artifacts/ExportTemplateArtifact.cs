using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Deploy;

namespace Vendr.Deploy.Artifacts
{
    public class ExportTemplateArtifact : StoreEntityArtifactBase
    {
        public ExportTemplateArtifact(GuidUdi udi, GuidUdi storeUdi, IEnumerable<ArtifactDependency> dependencies = null)
            : base(udi, storeUdi, dependencies)
        { }

        public int Category { get; set; }

        public string FileMimeType { get; set; }

        public string FileExtension { get; set; }

        public int ExportStrategy { get; set; }

        public string TemplateView { get; set; }

        public int SortOrder { get; set; }
    }
}
