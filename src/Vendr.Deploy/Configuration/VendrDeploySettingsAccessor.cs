using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace Vendr.Deploy.Configuration
{
    public class VendrDeploySettingsAccessor
    {
        private readonly IServiceProvider _serviceProvider;

        public VendrDeploySettingsAccessor(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public VendrDeploySettings Settings => _serviceProvider.GetRequiredService<IOptions<VendrDeploySettings>>().Value;
    }
}
