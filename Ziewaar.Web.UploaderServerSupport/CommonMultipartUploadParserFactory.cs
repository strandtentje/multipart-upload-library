using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ziewaar.Web.UploaderServerSupport
{
    public class CommonMultipartUploadParserFactory : IMultipartUploadParserFactory
    {
        public IMultipartUploadParser Create(
            byte[] boundary, string targetDirectory)
        {
            var forReceiving = BufferScheduler.CreatePreAllocated(6, 16 * 1024);
            var forProcessing = BufferScheduler.CreateMeasuring(6);
            var ingestor = new CommonMultipartComponentIngestor(
                boundary, targetDirectory);
            return new CommonMultipartUploadParser(
                forReceiving, forProcessing, ingestor);
        }
    }
}
