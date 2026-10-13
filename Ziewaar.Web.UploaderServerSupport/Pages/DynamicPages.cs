using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Ejije.Logging;

namespace Ziewaar.Web.UploaderServerSupport
{
    public class DynamicPages
    {
        public static readonly DynamicPage 
            UploadForm = DynamicPage.For(
            Path.Combine(SpecialPaths.AssemblyDirectory, "Html/200-UploadPage.html")),
            ProgressPage = DynamicPage.For(
            Path.Combine(SpecialPaths.AssemblyDirectory, "Html/210-ProgressPage.html"));
    }
}
