using System;

#if NETFRAMEWORK
using System.Configuration;
using System.Linq;
#endif

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
#if NETFRAMEWORK
            IgnoreSettings = (ConfigurationManager.AppSettings["Vendr.Deploy:PaymentMethods:IgnoreSettings"] ?? "")
                .Split(new[] { "," }, System.StringSplitOptions.RemoveEmptyEntries)
                .ToArray();
#else
            IgnoreSettings = Array.Empty<string>();
#endif
        }
    }
}
