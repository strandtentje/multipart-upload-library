using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ziewaar.Web.UploaderServerSupport
{
    public interface IMultipartFinishedComponentResult
    {
        string Name { get; }
        string[] Values { get; }
        bool IsFile { get; }
    }
}
