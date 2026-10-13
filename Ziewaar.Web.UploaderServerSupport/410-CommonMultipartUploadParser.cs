using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Ejije.Logging;

namespace Ziewaar.Web.UploaderServerSupport
{
    public class CommonMultipartUploadParser : IMultipartUploadParser
    {
        private readonly FixedBufferPool Receiving, Processing;
        private readonly VirtualInspectionBuffer Inspector;
        private readonly IMultipartComponentIngestor Ingestor;
        private readonly Thread WorkerThread;
        private readonly CommonMultipartUploadParserWorker Worker;

        public bool IsDisposing { get { return Worker.IsDisposing; } }
        public string Fault { get { return Worker.Fault; } }

        public CommonMultipartUploadParser(
            FixedBufferPool forReceiving, FixedBufferPool forProcessing,
            IMultipartComponentIngestor ingestor, int sleep)
        {
            this.Receiving = forReceiving;
            this.Processing = forProcessing;
            this.Ingestor = ingestor;
            this.Inspector = new VirtualInspectionBuffer();
            this.Worker = new CommonMultipartUploadParserWorker(
                this.Receiving, this.Processing,
                this.Inspector, this.Ingestor, sleep);
            this.WorkerThread = new Thread(this.Worker.ParserLoop);
            this.WorkerThread.Priority = ThreadPriority.BelowNormal;            
            this.WorkerThread.Start(null);
        }

        public byte[] PopForReceiving()
        {
            if (!string.IsNullOrEmpty(Fault))
                throw new InvalidOperationException(string.Format(
                    "Multipart Parse Fault: {0}", Fault));
            byte[] result;
            int count;
            while (!Receiving.TryDequeue(out result, out count))
            {
                if (IsDisposing)
                    throw new ObjectDisposedException(
                        "CommonMultipartUploadParser");
            }
            return result;
        }

        public void PushForProcessing(byte[] receivedBuffer, int count)
        {
            if (!string.IsNullOrEmpty(Fault))
                throw new InvalidOperationException(string.Format(
                    "Multipart Parse Fault: {0}", Fault));
            this.Processing.Enqueue(receivedBuffer, count);
        }

        public void Finish(
            IDictionary<string, IMultipartFinishedComponentResult> values,
            IList<string> faults)
        {
            this.Processing.Enqueue(FixedBuffer.Null.Allocation, 0);
            this.Processing.Enqueue(FixedBuffer.Null.Allocation, 0);
            this.WorkerThread.Join();
            this.Ingestor.Finish(values, faults);
        }

        public void Dispose()
        {
            try
            {
                this.Receiving.Dispose();
                this.Processing.Dispose();
            }
            finally
            {
                this.Ingestor.Dispose();
            }
        }
    }
}
