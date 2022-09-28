using System.Collections.Generic;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;

namespace Vendr.Deploy.Artifacts
{
    public class ProductAttributePresetArtifact : StoreEntityArtifactBase
    {
        public ProductAttributePresetArtifact(GuidUdi udi, GuidUdi storeUdi, IEnumerable<ArtifactDependency> dependencies = null)
            : base(udi, storeUdi, dependencies)
        { }

        public string Code { get; set; }

        public new string Alias
        {
            get => Code;
        }
        public string Icon { get; set; }
        public string Description { get; set; }

        public IEnumerable<AllowedProductAttributeArtifact> AllowedAttributes { get; set; }
        public int SortOrder { get; set; }
    }
}
