#if NETFRAMEWORK
using Umbraco.Core;
#else
using Umbraco.Cms.Core;
#endif

namespace Vendr.Deploy
{
    public static class UdiHelper
    {
        public static bool TryParseGuidUdi(string input, out GuidUdi udi)
        {
#if NETFRAMEWORK
            return GuidUdi.TryParse(input, out udi);
#else
            return UdiParser.TryParse<GuidUdi>(input, out udi);
#endif
        }
    }
}
