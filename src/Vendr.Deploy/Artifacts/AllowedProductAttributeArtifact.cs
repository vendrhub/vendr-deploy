using System.Collections.Generic;
using Umbraco.Cms.Core;

namespace Vendr.Deploy.Artifacts
{
    public class AllowedProductAttributeArtifact
    {
        public GuidUdi ProductAttributeUdi { get; set; }

        public IEnumerable<string> AllowedValueAliases { get; set; }
    }
}
