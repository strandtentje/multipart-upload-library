using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ziewaar.Web.UploaderServerSupport
{
    public class CommonMultipartUploadParserFactory : IMultipartUploadParserFactory
    {
        public IMultipartUploadParser Create(
            byte[] boundary, string targetDirectory, string[] acceptedFields)
        {
            var forReceiving = FixedBufferPool.CreatePreAllocated(6, 16 * 1024);
            var forProcessing = FixedBufferPool.CreateMeasuring(6);
            var ingestor = new CommonMultipartComponentIngestor(
                boundary, targetDirectory, acceptedFields);
            return new CommonMultipartUploadParser(
                forReceiving, forProcessing, ingestor);
        }
    }
}
