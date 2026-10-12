using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Ziewaar.Web.UploaderServerSupport
{
    public class HttpHeaderCollection : IEnumerable<HttpHeader> 
    {
        private readonly IEnumerable<HttpHeader> Backing;
        public HttpHeaderCollection(IEnumerable<HttpHeader> members)
        {
            this.Backing = members;
        }

        public static HttpHeaderCollection Parse(string headerText)
        {
            List<HttpHeader> list = new List<HttpHeader>();
            using (var reader = new StringReader(headerText))
                for (var line = reader.ReadLine();
                    !string.IsNullOrEmpty(line);
                    line = reader.ReadLine())
                    list.Add(HttpHeader.Parse(line));
            return new HttpHeaderCollection(list);
        }

        public IEnumerator<HttpHeader> GetEnumerator()
        {
            return Backing.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
