using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ziewaar.Web.UploaderServerSupport
{
    public class UploadBoxComparerById : IEqualityComparer<UploadBox>
    {
        public bool Equals(UploadBox x, UploadBox y)
        {
            return
                StringComparer.Ordinal.Equals(x.SessionId, y.SessionId) &&
                StringComparer.Ordinal.Equals(x.UploadBoxId, y.UploadBoxId);
        }

        public int GetHashCode(UploadBox obj)
        {
            return obj.SessionId.GetHashCode() ^ obj.UploadBoxId.GetHashCode();
        }
    }
}
