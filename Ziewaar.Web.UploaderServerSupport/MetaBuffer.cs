using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ziewaar.Web.UploaderServerSupport
{
    public class MetaBuffer
    {
        public static readonly MetaBuffer Null = new MetaBuffer(new byte[0], 0);

        public readonly byte[] Allocation;
        public readonly int Count;

        public MetaBuffer(byte[] allocation, int count)
        {
            this.Allocation = allocation;
            this.Count = count;
        }
    }
}
