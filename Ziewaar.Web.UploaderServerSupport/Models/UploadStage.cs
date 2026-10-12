using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ziewaar.Web.UploaderServerSupport
{
    public enum UploadStage
    {
        AmountSelection = 0,
        FileSelection = 1,
        FileUploading = 2,
        UploadDone = 3,
    }
}
