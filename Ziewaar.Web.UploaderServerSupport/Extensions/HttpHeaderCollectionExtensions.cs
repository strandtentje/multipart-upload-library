using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ziewaar.Web.UploaderServerSupport
{
    public static class HttpHeaderCollectionExtensions
    {
        public static HttpHeader ByName(this HttpHeaderCollection collection, string name)
        {
            var matches = collection.
                Where(x => StringComparer.OrdinalIgnoreCase.Equals(x.Name, name));
            return matches.ElementAtOrDefault(0);
        }
    }
}
