using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ziewaar.Web.UploaderServerSupport
{
    public interface IContentTypeValidator
    {
        bool TryValidateContentType(string contentType);
    }
}
