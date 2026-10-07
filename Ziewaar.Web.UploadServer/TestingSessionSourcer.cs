using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ziewaar.Web.UploaderServerSupport;
using System.Net;

namespace Ziewaar.Web.UploadServer
{
    public class TestingSessionSourcer : ISessionSourcer
    {
        public string GetSessionIdFor(HttpListenerRequest req)
        {
            return "TEST";
        }
    }
}
