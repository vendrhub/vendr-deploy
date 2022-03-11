using System;

#if NETFRAMEWORK
using System.Configuration;
using System.Linq;
#endif

namespace Vendr.Deploy.Configuration
{
    public class VendrDeploySettings
    {
        public VendrDeployPaymentProviderSettings PaymentProviders { get; set; }

        public VendrDeploySettings()
        {
            PaymentProviders = new VendrDeployPaymentProviderSettings();
        }
    }

    public class VendrDeployPaymentProviderSettings
    {
        public string[] IgnoreSettings { get; set; }

        public VendrDeployPaymentProviderSettings()
        {
#if NETFRAMEWORK
            IgnoreSettings = (ConfigurationManager.AppSettings["Vendr.Deploy:PaymentProviders:IgnoreSettings"] ?? "")
                .Split(new[] { "," }, System.StringSplitOptions.RemoveEmptyEntries)
                .ToArray();
#else
            IgnoreSettings = Array.Empty<string>();
#endif
        }
    }
}
