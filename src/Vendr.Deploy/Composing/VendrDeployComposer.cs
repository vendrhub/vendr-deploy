using System.Collections.Generic;
using System.Reflection;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Deploy;

namespace Vendr.Deploy.Composing
{
    [ComposeAfter(typeof(DeployComposer))]
    public class VendrDeployComposer : IUserComposer
    {
        public void Compose(Composition composition)
        {
            //var prop = typeof(DeployConstants).GetProperty("DeployValidEntityTypes", BindingFlags.Static | BindingFlags.NonPublic);
            //if (prop != null && prop.CanRead)
            //{
            //    if (prop.GetValue(null) is IList<string> list)
            //    {
            //        list.Add(VendrConstants.UdiEntityType.ProductAttribute);
            //    }
            //}

            composition.Components()
                .Append<VendrDeployComponent>();
        }
    }
}
