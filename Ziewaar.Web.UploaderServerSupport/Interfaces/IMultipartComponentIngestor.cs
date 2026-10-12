using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ziewaar.Web.UploaderServerSupport
{
    public interface IMultipartComponentIngestor
    {
        bool IsExpectingContent { get; }

        bool TryHaveBoundary(out byte[] boundary);
        void GiveBinaryData(byte[] bytes, int start, int count);

        void RetireBoundary();

        bool GiveHeaderString(string headerString);

        void Finish();

        void Clean();
    }
}
