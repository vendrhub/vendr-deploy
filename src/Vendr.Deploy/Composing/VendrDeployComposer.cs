using Vendr.Deploy.Configuration;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Vendr.Deploy.Composing
{
    public class VendrDeployComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services.AddOptions<VendrDeploySettings>()
                .Bind(builder.Config.GetSection("Vendr.Deploy"));
            builder.Services.AddSingleton<VendrDeploySettingsAccessor>();

            builder.Components()
                .Append<VendrDeployComponent>();
        }
    }
}
