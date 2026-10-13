using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ziewaar.Web.UploaderServerSupport
{
    public class CommonMultipartUploadParserFactory : IMultipartUploadParserFactory
    {
        private readonly IContentTypeValidator ContentTypeValidator;
        public CommonMultipartUploadParserFactory(
            IContentTypeValidator contentTypeValidator)
        {
            this.ContentTypeValidator = contentTypeValidator;
        }
        public IMultipartUploadParser Create(
            byte[] boundary, string targetDirectory, string[] acceptedFields)
        {
            var forReceiving = FixedBufferPool.CreatePreAllocated(6, 16 * 1024);
            var forProcessing = FixedBufferPool.CreateMeasuring(6);
            var ingestor = new CommonMultipartComponentIngestor(
                boundary, targetDirectory, acceptedFields, ContentTypeValidator);
            return new CommonMultipartUploadParser(
                forReceiving, forProcessing, ingestor);
        }
    }
}
