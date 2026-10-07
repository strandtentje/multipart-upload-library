using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Ziewaar.Web.UploaderServerSupport
{
    public interface IUploadReceiver
    {
        bool TryValidateUpload(HttpListenerRequest req, 
            HttpListenerResponse rsp, UploadBox selectedUploadBox);

        bool TryReceiveUpload(HttpListenerRequest req, UploadBox selectedUploadBox);
    }
}
