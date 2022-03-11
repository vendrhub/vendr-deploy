#if NET
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
#endif

namespace Vendr.Deploy.Configuration
{
    public class VendrDeploySettingsAccessor
    {
#if NETFRAMEWORK
        public VendrDeploySettings Settings { get; }

        public VendrDeploySettingsAccessor (VendrDeploySettings settings)
	    {
            Settings = settings;
	    }
#else
        private readonly IServiceProvider _serviceProvider;

        public VendrDeploySettingsAccessor(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public VendrDeploySettings Settings => _serviceProvider.GetRequiredService<IOptions<VendrDeploySettings>>().Value;
#endif
    }
}
