using System.Collections.Generic;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;

namespace Vendr.Deploy.Artifacts
{
    public class ProductAttributeArtifact : StoreEntityArtifactBase
    {
        public ProductAttributeArtifact(GuidUdi udi, GuidUdi storeUdi, IEnumerable<ArtifactDependency> dependencies = null)
            : base(udi, storeUdi, dependencies)
        { }

        public string Code { get; set; }

        public new string Alias
        {
            get => Code;
        }

        public new TranslatedValueArtifact<string> Name { get; set; }
        public IEnumerable<ProductAttributeValueArtifact> Values { get; set; }
        public int SortOrder { get; set; }
    }

    public class ProductAttributeValueArtifact
    {
        public string Alias { get; set; }
        public TranslatedValueArtifact<string> Name { get; set; }
    }

    public class TranslatedValueArtifact<T>
    {
        public SortedDictionary<string, T> Translations { get; set; }
        public T DefaultValue { get; set; }
    }
}
