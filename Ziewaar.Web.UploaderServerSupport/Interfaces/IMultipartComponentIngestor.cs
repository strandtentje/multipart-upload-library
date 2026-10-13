using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ziewaar.Web.UploaderServerSupport
{
    public interface IMultipartComponentIngestor : IDisposable
    {
        bool IsExpectingContent { get; }

        bool TryHaveBoundary(out byte[] boundary);
        void GiveBinaryData(byte[] bytes, int start, int count);

        bool TryRetireBoundary();

        bool GiveHeaderString(string headerString);

        void Finish(
            IDictionary<string, IMultipartFinishedComponentResult> values,
            IList<string> faults);
    }
}
