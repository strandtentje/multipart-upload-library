using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ziewaar.Web.UploaderServerSupport
{
    public class CommonMultipartUploadParserWorker : IDisposable
    {
        private readonly BufferScheduler Receiving, Processing;
        private readonly VirtualInspectionBuffer Inspector;
        private readonly IMultipartComponentIngestor Ingestor;
        private readonly MultipartByteSequenceGovernor Boundaries;
        private readonly MultipartPositionsGovernor Positions;
        public bool IsDisposing { get; private set; }
        public string Fault { get; private set; }

        public CommonMultipartUploadParserWorker(
            BufferScheduler receiving, BufferScheduler processing,
            VirtualInspectionBuffer inspector,
            IMultipartComponentIngestor ingestor)
        {
            this.Receiving = receiving;
            this.Processing = processing;
            this.Inspector = inspector;
            this.Ingestor = ingestor;
        }

        public void ParserLoop(object arg)
        {
            byte[] currentBoundary;

            while (!this.Inspector.IsEof && this.Ingestor.TryHaveBoundary(
                out currentBoundary))
            {
                Boundaries.Rescope(currentBoundary);
                if (TryPushBuffers())
                    ProcessNewBuffers();
            }
        }

        private bool TryPushBuffers()
        {
            byte[] availableData;
            int availableDataCount;
            if (!this.Processing.TryDequeue(out availableData, out availableDataCount))
                return false;

            var reusableBuffer = this.Inspector.Push(availableData, availableDataCount);
            if (reusableBuffer.Length > 0)
                Receiving.Enqueue(reusableBuffer, reusableBuffer.Length);

            return true;
        }

        private bool ProcessNewBuffers()
        {
            ByteSequenceNodeTag tag;
            while (Positions.ConsumedUntil < this.Inspector.WindowEnd)
            {
                if (!this.Inspector.IndexOf(Boundaries.Current,
                    Positions.ConsumedUntil,
                    out Positions.BoundaryStart,
                    out Positions.BoundaryEnd, out tag))
                {
                    if (this.Ingestor.IsExpectingContent &&
                        Positions.ContentStart > 0)
                        FlushWindow(
                            Positions.ContentStart, this.Inspector.WindowEnd);
                    Positions.Break = this.Inspector.WindowEnd;
                    Boundaries.WithCrlf();
                    continue;
                }
                else
                {
                    Positions.ContentEnd = Positions.BoundaryStart;
                }

                if (this.Ingestor.IsExpectingContent &&
                    Positions.ContentStart > 0)
                    FlushWindow(Positions.ContentStart, Positions.ContentEnd);

                if (!ProcessBoundary(tag))
                    return false;
            }
            return true;
        }

        private bool ProcessBoundary(ByteSequenceNodeTag tag)
        {
            switch (tag)
            {
                case ByteSequenceNodeTag.DashDashBoundaryDashDash:
                    this.Ingestor.RetireBoundary();
                    Boundaries.WithoutCrlf();
                    Positions.ContentStart = 0;
                    break;

                case ByteSequenceNodeTag.DashDashBoundaryCrLf:
                    Positions.HeaderEnd = Positions.BoundaryEnd;
                    if (!this.Inspector.IndexOf(
                        ByteSequenceNode.DoubleCrlf, Positions.HeaderStart,
                        out Positions.HeaderEnd, out Positions.ContentStart, out tag) ||
                        tag != ByteSequenceNodeTag.SecondHeaderCrlf)
                    {
                        Fault = string.Format(
                            "Expected double CRLF denoting end of header " +
                            "section, but it wasn't found at {0}",
                            Positions.BoundaryEnd);
                        return false;
                    }
                    else if (this.Ingestor.GiveHeaderString(this.Inspector.ExtractString(
                            Positions.HeaderStart, Positions.HeaderEnd)))
                    {
                        Boundaries.WithCrlf();
                    }
                    else
                    {
                        Fault = string.Format("Ingestor failed at; {0}",
                            Positions.ConsumedUntil);
                    }

                    break;

                default:
                    Fault = string.Format(
                        "Unexpected multipart directive; {0}",
                        tag.ToString());
                    return false;
            }
            return true;
        }

        private void FlushWindow(long contentStart, long contentEnd)
        {
            var readingWindowDataStart = contentStart - this.Inspector.WindowOffset;
            readingWindowDataStart = Math.Max(0, readingWindowDataStart);
            var readingWindowDataEnd = contentEnd - this.Inspector.WindowOffset;
            var readingWindowDataCount =
                readingWindowDataEnd - readingWindowDataStart;

            if (readingWindowDataCount > this.Inspector.ReadingWindow.Count)
            {
                throw new InvalidOperationException(
                    "Remaining data stretched outside ReadingWindow");
            }
            if (readingWindowDataCount > 0)
            {
                this.Ingestor.GiveBinaryData(
                    this.Inspector.ReadingWindow.Allocation,
                    (int)readingWindowDataStart, (int)readingWindowDataCount);
            }
        }

        public void Dispose()
        {
            this.IsDisposing = true;
        }
    }
}
