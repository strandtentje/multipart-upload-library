using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Ejije.Logging;

namespace Ziewaar.Web.UploadServer
{
    public class Webserver : IDisposable
    {
        private static readonly Log
            StartServer = Log.Cool("Server start"),
            RequestRecv = Log.Info("Request receive {method} {route}"),
            MissingAsyncResult = Log.Oops("Missing async result"),
            FailedGetCtx = Log.Fail("Failed to get context {dueto}"),
            FailedNewCtx = Log.Fail("Failed to start getting new ctx {dueto}"),
            NoEventHandler = Log.Fail("No event handler configured for request"),
            RequestHandlerFail = Log.Fail(
            "Uncaught {exception} during request handling");
        private readonly HttpListener Listener;
        public event WebserverEventHandler Request;
        public event WebserverEventHandler Favico;
        public Webserver(string prefix)
        {
            Listener = new HttpListener();
            Listener.Prefixes.Add("http://+:8243/");
            Listener.Start();
            Log.Post(StartServer);
            Listener.BeginGetContext(ReceiveRequest, Listener);
        }
        private void ReceiveRequest(IAsyncResult r)
        {
            var listener = r.AsyncState as HttpListener;
            if (listener == null)
            {
                Log.Post(MissingAsyncResult);
                return;
            }
            HttpListenerContext ctx;
            bool isFavico = false;
            try
            {
                ctx = listener.EndGetContext(r);
                isFavico = ctx.Request.RawUrl.StartsWith("/favico",
                    StringComparison.OrdinalIgnoreCase);
                if (!isFavico)
                    Log.Post(RequestRecv, ctx.Request.HttpMethod, ctx.Request.RawUrl);
            }
            catch (Exception ex)
            {
                Log.Post(FailedGetCtx, ex);
                return;
            }
            finally
            {
                try
                {
                    listener.BeginGetContext(ReceiveRequest, listener);
                }
                catch (Exception ex2)
                {
                    Log.Post(FailedNewCtx, ex2);
                }
            }
            using (ctx.Request.InputStream)
            using (ctx.Response.OutputStream)
            {

                try
                {
                    if (!isFavico)
                    {
                        if (Request == null)
                        {
                            Log.Post(NoEventHandler);
                        }
                        else
                        {
                            Request.Invoke(this, new WebserverEventArgs(ctx));
                        }
                    }
                    else
                    {
                        if (Favico != null)
                        {
                            Favico.Invoke(this, new WebserverEventArgs(ctx));
                        }
                    }

                }
#if !DEBUG
                catch (Exception ex)
                {
                    Log.Post(RequestHandlerFail, ex);
                }
#endif
                finally
                {
                    ctx.Request.InputStream.Close();
                    ctx.Response.OutputStream.Flush();
                    ctx.Response.Close();
                }
            }
        }
        public void Dispose()
        {
            Listener.Stop();
        }
    }
}
