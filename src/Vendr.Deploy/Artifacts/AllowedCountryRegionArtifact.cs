﻿using Umbraco.Core;

namespace Vendr.Deploy.Artifacts
{
    public class AllowedCountryRegionArtifact: AllowedCountryArtifact
    {
        public GuidUdi RegionUdi { get; set; }
    }
}
