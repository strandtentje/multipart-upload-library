using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Ziewaar.Web.UploaderServerSupport
{
    public interface IProgressGrapher
    {
        void Run(UploadBox selectedUploadBox, HttpListenerResponse rsp);
    }
}
