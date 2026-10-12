using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ziewaar.Web.UploaderServerSupport
{
    public enum ByteSequenceNodeTag
    {
        StartingCrlf,
        StartingDashDash, 
        Boundary,
        DashDashBoundaryDashDash,
        DashDashBoundaryCrLf,
        FirstHeaderCrlf,
        SecondHeaderCrlf,
    }
}
