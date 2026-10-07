using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ziewaar.Web.UploaderServerSupport
{
    public interface IPlaceholderResolver
    {
        long GetByteLengthFor(string p);

        void WriteBytesFor(string p, System.IO.Stream stream);
    }
}
