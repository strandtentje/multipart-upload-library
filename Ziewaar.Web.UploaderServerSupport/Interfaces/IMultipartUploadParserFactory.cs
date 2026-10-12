using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ziewaar.Web.UploaderServerSupport
{
    public interface IMultipartUploadParserFactory
    {
        IMultipartUploadParser Create(
            byte[] boundary, 
            string targetDirectory,
            string[] acceptedFields);
    }
}
