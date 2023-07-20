﻿using System.Collections.Generic;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;

namespace Vendr.Deploy.Artifacts
{
    public class PaymentMethodArtifact : StoreEntityArtifactBase
    {
        public PaymentMethodArtifact(GuidUdi udi, GuidUdi storeUdi, IEnumerable<ArtifactDependency> dependencies = null)
            : base(udi, storeUdi, dependencies)
        { }

        public string Sku { get; set; }
        public GuidUdi TaxClassUdi { get; set; }
        public IEnumerable<ServicePriceArtifact> Prices { get; set; }
        public string ImageId { get; set; }

        public string PaymentProviderAlias { get; set; }
        public SortedDictionary<string, string> PaymentProviderSettings { get; set; }
        public bool CanFetchPaymentStatuses { get; set; }
        public bool CanCapturePayments { get; set; }
        public bool CanCancelPayments { get; set; }
        public bool CanRefundPayments { get; set; }

        public IEnumerable<AllowedCountryRegionArtifact> AllowedCountryRegions { get; set; }
        public int SortOrder { get; set; }
    }
}
