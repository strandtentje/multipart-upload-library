using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ziewaar.Web.UploaderServerSupport;

namespace Ziewaar.Web.UploadServer
{
    public class TestingContentTypeValidator : IContentTypeValidator
    {
        public bool TryValidateContentType(string contentType)
        {
            return true;
        }
    }
}
