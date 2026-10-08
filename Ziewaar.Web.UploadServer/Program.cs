using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Ejije.Logging;
using Ziewaar.Web.UploaderServerSupport;
using Ziewaar.Web.GdiProgressGrapher;

namespace Ziewaar.Web.UploadServer
{
    class Program
    {
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
                    new CommonMultipartUploadParserFactory()),
                BoxRepository);

            using (var server = new Webserver("http://+:8243/"))
            {
                server.Request += new WebserverEventHandler(server_Request);
                for (var cmd = Console.ReadLine();
                    !StringComparer.OrdinalIgnoreCase.Equals(cmd, "quit");
                    cmd = Console.ReadLine())
                {
                    switch (cmd)
                    {
                        case "nb":
                            var nb = BoxRepository.Enter("TEST",
                                Environment.GetFolderPath(
                                    Environment.SpecialFolder.MyDocuments));
                            Log.Post(NewBoxId, nb.UploadBoxId);
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
