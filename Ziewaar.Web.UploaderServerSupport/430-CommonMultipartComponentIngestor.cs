using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Ziewaar.Web.UploaderServerSupport
{
    public class CommonMultipartComponentIngestor : IMultipartComponentIngestor
    {
        private readonly Stack<byte[]> Boundaries = new Stack<byte[]>();
        private readonly List<IMultipartComponentResult> FinishableResults =
            new List<IMultipartComponentResult>();
        private IMultipartComponentResult UnfinishedResult;
        private readonly string[] AcceptedFields;
        private readonly string TargetDirectory;

        public CommonMultipartComponentIngestor(byte[] boundary,
            string targetDirectory, string[] acceptedFields)
        {
            this.Boundaries.Push(boundary);
            this.TargetDirectory = targetDirectory;
            this.AcceptedFields = acceptedFields;
            this.UnfinishedResult = new FormStartComponentResult();
        }

        public bool IsExpectingContent 
        { 
            get 
            { 
                return UnfinishedResult.IsExpectingContent; 
            } 
        }

        public bool TryHaveBoundary(out byte[] boundary)
        {
            if (Boundaries.Any())
            {
                boundary = Boundaries.Peek();
                return true;
            }
            else
            {
                boundary = new byte[0];
                return false;
            }
        }

        public bool GiveHeaderString(string headerString)
        {
            var headers = HttpHeaderCollection.Parse(headerString);
            var contentDisposition = headers.ByName(HeaderNames.ContentDisposition);
            var contentType = headers.ByName(HeaderNames.ContentType);
            var transferEncoding = headers.ByName(
                HeaderNames.ContentTransferEncoding);

            if (contentDisposition.HasMainValueCI("form-data"))
                return GiveFormDataHeader(
                    contentDisposition, contentType, transferEncoding);

            if (contentDisposition.HasMainValueCI("file"))
                return GiveSubFileHeader(
                    contentDisposition, contentType, transferEncoding);

            FinishableResults.Add(new MultipartComponentFaultyResult(
                    "Unsupported or Invalid Content-Disposition"));
            return false;
        }

        private bool GiveFormDataHeader(HttpHeader contentDispositionHeader,
            HttpHeader contentTypeHeader, HttpHeader transferEncodingHeader)
        {
            var fieldName = contentDispositionHeader.GetPropValueCI("name");
            if (!AcceptedFields.Contains(fieldName, StringComparer.OrdinalIgnoreCase))
            {
                FinishableResults.Add(new MultipartComponentFaultyResult(
                    "Missing name from content disposition"));
                return false;
            }
            var fileName = contentDispositionHeader.GetPropValueCI("filename");
            if (fileName == null && contentTypeHeader == null)
            {
                FinishableResults.Add(UnfinishedResult);
                UnfinishedResult = new MultipartInputFieldResult(fieldName);
                return true;
            }

        }

        private bool GiveSubFileHeader(HttpHeader contentDisposition,
            HttpHeader contentType, HttpHeader transferEncoding)
        {
            throw new NotImplementedException();
        }

        public void GiveBinaryData(byte[] bytes, int start, int count)
        {

        }

        public void RetireBoundary()
        {
            throw new NotImplementedException();
        }


        public void Finish()
        {
            throw new NotImplementedException();
        }

        public void Clean()
        {
            throw new NotImplementedException();
        }
    }
}
