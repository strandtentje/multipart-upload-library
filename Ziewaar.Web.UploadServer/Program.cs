using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Ejije.Logging;
using Ziewaar.Web.UploaderServerSupport;
using Ziewaar.Web.GdiProgressGrapher;
using System.Diagnostics;

namespace Ziewaar.Web.UploadServer
{
    class Program
    {
        private const string BROWSER_EXE = @"D:\mypal-78.0.2.en-US.win32\mypal\mypal.exe";
        private static readonly Log
            UnknownCommand =
            Log.Shit("Unknown command; I understand: `quit`, `nb`"),
            NewBoxId =
            Log.Cool("New {uploadboxid}");
        private static UploadRouter Router;
        private static UploadBoxRepository BoxRepository;
        static void Main(string[] args)
        {
            BoxRepository = new UploadBoxRepository();
            Router = new UploadRouter(
                new TestingSessionSourcer(),
                new TestingPlaceholderResolver(),
                new GdiProgressGrapher.GdiProgressGrapher(150, 32),
                new CommonUploadReceiver(
                    new CommonUploadSizeGovernor(
                        8 * CommonUploadSizeGovernor.GB,
                        60 * CommonUploadSizeGovernor.GB), 
                    new CommonMultipartUploadParserFactory(
                        new TestingContentTypeValidator())),
                BoxRepository);

            using (var server = new Webserver("http://+:8243/"))
            {
                server.Request += new WebserverEventHandler(server_Request);
                for (var cmd = Console.ReadLine();
                    !StringComparer.OrdinalIgnoreCase.Equals(cmd, "quit");
                    cmd = Console.ReadLine())
                {
                    var myDocs =
                        Environment.GetFolderPath(
                            Environment.SpecialFolder.MyDocuments);

                    switch (cmd)
                    {
                        case "nb":
                            var nb = BoxRepository.Enter(
                                "TEST", myDocs, new string[] { "files" });
                            
                            Log.Post(NewBoxId, nb.UploadBoxId);
                            break;
                        case "nbx":
                            var nbx = BoxRepository.Enter(
                                "TEST", myDocs, new string[] { "files" });
                            var nburl = string.Format(
                                "http://{0}/?uploadBoxId={1}&functionId=COMBO",
                                "localhost:8243", nbx.UploadBoxId);
                            Log.Post(NewBoxId, nbx.UploadBoxId);
                            Process.Start(BROWSER_EXE, nburl);
                            break;
                        default:
                            Log.Post(UnknownCommand);
                            break;
                    }
                }
            }
        }

        static void server_Request(object sender, WebserverEventArgs e)
        {
            Router.Route(e.Context.Request, e.Context.Response);
        }

    }
}
