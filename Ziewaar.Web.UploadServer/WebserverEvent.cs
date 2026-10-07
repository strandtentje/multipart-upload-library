using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Ziewaar.Web.UploadServer
{
    public delegate void WebserverEventHandler(object sender, WebserverEventArgs e);
    public class WebserverEventArgs : EventArgs
    {
        public readonly HttpListenerContext Context;
        public WebserverEventArgs(HttpListenerContext ctx)
        {
            this.Context = ctx;
        }
    }
}
