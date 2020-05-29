using Umbraco.Core;
using Umbraco.Core.Composing;

namespace Vendr.Deploy.Composing
{
    public class VendrDeployComposer : IUserComposer
    {
        public void Compose(Composition composition)
        {
            composition.Components()
                .Append<VendrDeployComponent>();
        }
    }
}
