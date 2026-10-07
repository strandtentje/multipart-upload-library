using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ziewaar.Web.UploaderServerSupport;
using System.IO;

namespace Ziewaar.Web.UploadServer
{
    public class TestingPlaceholderResolver : IPlaceholderResolver
    {
        public long GetByteLengthFor(string p)
        {
            if (p == "url")
            {
                return 1;
            }
            else
            {
                return p.Length + 4;
            }
        }

        public void WriteBytesFor(string p, Stream stream)
        {
            if (p == "url")
            {
                var b = Encoding.UTF8.GetBytes("/");
                stream.Write(b, 0, b.Length);
            }
            else
            {
                var b = Encoding.UTF8.GetBytes(p + "TEST");
                stream.Write(b, 0, b.Length);
            }
        }
    }
}
