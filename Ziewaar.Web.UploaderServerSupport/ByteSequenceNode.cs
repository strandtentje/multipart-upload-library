using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ziewaar.Web.UploaderServerSupport
{
    public class ByteSequenceNode
    {
        private static readonly byte[]
            CrLf = Encoding.ASCII.GetBytes("\r\n"),
            DashDash = Encoding.ASCII.GetBytes("--");

        public readonly ByteSequenceNodeTag Tag;
        public byte[] Sequence;
        public readonly bool CanEndHere;
        public readonly ByteSequenceNode[] NextOptions;

        public ByteSequenceNode(
            ByteSequenceNodeTag tag,
            byte[] sequence,
            bool canEndHere,
            params ByteSequenceNode[] nextOptions)
        {
            this.Tag = tag;
            this.Sequence = sequence;
            this.CanEndHere = canEndHere;
            this.NextOptions = nextOptions;
        }

        public static readonly ByteSequenceNode
            DoubleCrlf = WrapCrlf(new ByteSequenceNode(
                    ByteSequenceNodeTag.SecondHeaderCrlf, CrLf, true));

        public static ByteSequenceNode WrapDashDash(ByteSequenceNode child)
        {
            return new ByteSequenceNode(
                ByteSequenceNodeTag.StartingDashDash, DashDash, false, child);
        }

        public static ByteSequenceNode WrapCrlf(ByteSequenceNode child)
        {
            return new ByteSequenceNode(
                ByteSequenceNodeTag.StartingCrlf, DashDash, false, child);
        }

        public static ByteSequenceNode CreateBoundaryTree()
        {
            return
                new ByteSequenceNode(
                    ByteSequenceNodeTag.Boundary, new byte[0], false,
                    new ByteSequenceNode(
                        ByteSequenceNodeTag.DashDashBoundaryCrLf, CrLf, true),
                    new ByteSequenceNode(
                        ByteSequenceNodeTag.DashDashBoundaryDashDash, DashDash, true,
                        new ByteSequenceNode(
                            ByteSequenceNodeTag.DashDashBoundaryDashDash, CrLf, true)));
        }
    }
}
