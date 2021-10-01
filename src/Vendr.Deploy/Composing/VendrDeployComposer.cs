#if NETFRAMEWORK
using Umbraco.Deploy;
using Umbraco.Core;
using Umbraco.Core.Composing;
using IComposer = Umbraco.Core.Composing.IUserComposer;
#else
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
#endif

namespace Vendr.Deploy.Composing
{
#if NETFRAMEWORK
    [ComposeAfter(typeof(DeployComposer))]
#endif
    public class VendrDeployComposer : IComposer
    {
#if NETFRAMEWORK
        public void Compose(Composition composition)
        {
            composition.Components()
                .Append<VendrDeployComponent>();
        }
#else
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Components()
                .Append<VendrDeployComponent>();
        }
#endif
    }
}
