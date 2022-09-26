using System.Collections.Generic;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;

namespace Vendr.Deploy.Artifacts
{
    public class EmailTemplateArtifact : StoreEntityArtifactBase
    {
        public EmailTemplateArtifact(GuidUdi udi, GuidUdi storeUdi, IEnumerable<ArtifactDependency> dependencies = null)
            : base(udi, storeUdi, dependencies)
        { }

        public int Category { get; set; }

        public bool SendToCustomer { get; set; }

        public string Subject { get; set; }

        public string SenderName { get; set; }

        public string SenderAddress { get; set; }

        public IEnumerable<string> ToAddresses { get; set; }

        public IEnumerable<string> CcAddresses { get; set; }

        public IEnumerable<string> BccAddresses { get; set; }

        public string TemplateView { get; set; }

        public int SortOrder { get; set; }
    }
}
