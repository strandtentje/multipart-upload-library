using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ziewaar.Web.UploaderServerSupport
{
    public interface IMultipartComponentResult : IDisposable
    {
        string Name { get; }
        void GiveBinaryData(byte[] data, int start, int length);
        IMultipartFinishedComponentResult Finish();
        string Fault { get; }
        bool IsExpectingContent { get; }
    }
}
