using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ziewaar.Web.UploaderServerSupport
{
    public class CommonMultipartUploadParserFactory : IMultipartUploadParserFactory
    {
        private readonly IContentTypeValidator ContentTypeValidator;
        private readonly ITransferRateConstraints TransferRateConstraints;

        public CommonMultipartUploadParserFactory(
            IContentTypeValidator contentTypeValidator, 
            ITransferRateConstraints transferRateConstraints)
        {
            this.ContentTypeValidator = contentTypeValidator;
            this.TransferRateConstraints = transferRateConstraints;
        }
        public IMultipartUploadParser Create(
            byte[] boundary, string targetDirectory, string[] acceptedFields)
        {
            var forReceiving = FixedBufferPool.CreatePreAllocated(6, TransferRateConstraints.BufferSize);
            var forProcessing = new FixedBufferPool(6);
            var ingestor = new CommonMultipartComponentIngestor(
                boundary, targetDirectory, acceptedFields, ContentTypeValidator);
            return new CommonMultipartUploadParser(
                forReceiving, forProcessing, ingestor,
                this.TransferRateConstraints.Sleep);
        }
    }
}
