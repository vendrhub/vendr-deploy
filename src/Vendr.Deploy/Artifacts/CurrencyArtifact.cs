﻿using System.Collections.Generic;

#if NETFRAMEWORK
using Umbraco.Core;
using Umbraco.Core.Deploy;
#else
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
#endif

namespace Vendr.Deploy.Artifacts
{
    public class CurrencyArtifact : StoreEntityArtifactBase
    {
        public CurrencyArtifact(GuidUdi udi, GuidUdi storeUdi, IEnumerable<ArtifactDependency> dependencies = null)
            : base(udi, storeUdi, dependencies)
        { }

        public string Code { get; set; }

        public new string Alias
        {
            get => Code;
        }

        public string CultureName { get; set; }
        public string FormatTemplate { get; set; }
        public IEnumerable<AllowedCountryArtifact> AllowedCountries { get; set; }
        public int SortOrder { get; set; }
    }
}
