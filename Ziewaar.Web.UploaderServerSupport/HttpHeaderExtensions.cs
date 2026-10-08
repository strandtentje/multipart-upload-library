using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ziewaar.Web.UploaderServerSupport
{
    public static class HttpHeaderExtensions
    {
        public static bool HasMainValueCI(
            this HttpHeader header,
            string value)
        {
            if (header == null) return false;
            return StringComparer.OrdinalIgnoreCase.Equals(
                header.MainValue, value);
        }
        public static string GetPropValueCI(this HttpHeader header, string name)
        {
            if (header == null) return null;
            return header.Where(x => StringComparer.OrdinalIgnoreCase.Equals(
                x.Name, name)).Select(x => x.Value).ElementAtOrDefault(0);
        }
    }
}
