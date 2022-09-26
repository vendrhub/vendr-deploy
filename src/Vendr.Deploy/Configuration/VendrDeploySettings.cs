using System;

namespace Vendr.Deploy.Configuration
{
    public class VendrDeploySettings
    {
        public VendrDeployPaymentMethodSettings PaymentMethods { get; set; }

        public VendrDeploySettings()
        {
            PaymentMethods = new VendrDeployPaymentMethodSettings();
        }
    }

    public class VendrDeployPaymentMethodSettings
    {
        public string[] IgnoreSettings { get; set; }

        public VendrDeployPaymentMethodSettings()
        {
            IgnoreSettings = Array.Empty<string>();
        }
    }
}
