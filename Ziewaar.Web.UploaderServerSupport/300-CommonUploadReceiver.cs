using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Ejije.Logging;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Ziewaar.Web.UploaderServerSupport
{
    public class CommonUploadReceiver : IUploadReceiver
    {
        private readonly IUploadSizeGovernor SizeGovernor;
        private readonly IMultipartUploadParserFactory ParserFactory;
        private readonly ITransferRateConstraints TransferRateConstraints;
        public CommonUploadReceiver(
            IUploadSizeGovernor sizeGovernor,
            IMultipartUploadParserFactory parserFactory,
            ITransferRateConstraints transferRateConstraints)
        {
            this.SizeGovernor = sizeGovernor;
            this.ParserFactory = parserFactory;
            this.TransferRateConstraints = transferRateConstraints;
        }
        private static readonly Log
            ValidatedUpload = Log.Tech(
            "Validated {uploadBoxId} into {targetDir} " + 
            "with {contentLength} and {boundary} " + 
            "having {temporaryFile}");
        private const string
            MULTIPART_FORM_DATA = "multipart/form-data",
            BOUNDARY_EQ = "boundary=";
        public bool TryValidateUpload(HttpListenerRequest req,
            HttpListenerResponse rsp, UploadBox selectedUploadBox)
        {
            var contentTypeParts = req.ContentType.Split(
                new char[] { ';' },
                2, StringSplitOptions.RemoveEmptyEntries).
                Select(x => x.Trim()).ToArray();
            var expectsMultipartFormData = contentTypeParts.ElementAtOrDefault(0);
            var expectsBoundary = contentTypeParts.ElementAtOrDefault(1);
            if (string.IsNullOrEmpty(expectsMultipartFormData) ||
                !StringComparer.OrdinalIgnoreCase.Equals(
                    expectsMultipartFormData,
                    MULTIPART_FORM_DATA) ||
                string.IsNullOrEmpty(expectsBoundary) ||
                !expectsBoundary.StartsWith(BOUNDARY_EQ,
                StringComparison.OrdinalIgnoreCase) ||
                (expectsBoundary.Length - BOUNDARY_EQ.Length) < 3 ||
                req.ContentLength64 < 1)
            {
                rsp.WriteUtf8Bytes(400, StaticPages.Instance.Pg140BadUploadStart);
                return false;
            }
            if (!SizeGovernor.ValidateContentLength(
                selectedUploadBox.TargetDirectory, req.ContentLength64))
            {
                rsp.WriteUtf8Bytes(413, StaticPages.Instance.Pg160UploadTooBig);
                return false;
            }
            var boundaryString = expectsBoundary.Substring(BOUNDARY_EQ.Length);
            if (!selectedUploadBox.TryInitiateUploading(
                req.ContentLength64,
                boundaryString))
            {
                rsp.WriteUtf8Bytes(409,
                    StaticPages.Instance.Pg150ConflictingUpload);
                return false;
            }
            Log.Post(ValidatedUpload,
                selectedUploadBox.UploadBoxId, selectedUploadBox.TargetDirectory,
                selectedUploadBox.ContentLengthAnnounced,
                boundaryString, selectedUploadBox.TempFile);
            return true;
        }

        private static readonly Log UploadFailure = Log.Fail(
            "Failed to finish upload {guid} to {target} due to {exception}");

        public bool TryReceiveUpload(HttpListenerRequest req,
            UploadBox selectedUploadBox)
        {
            try
            {
                using (var receiveParser = ParserFactory.Create(
                    selectedUploadBox.BoundaryBytes, 
                    selectedUploadBox.TargetDirectory,
                    selectedUploadBox.AcceptedFields))
                {
                    return ReceiveLoop(req, receiveParser, selectedUploadBox);
                }
            }
            catch (Exception ex)
            {
                Log.Post(UploadFailure, selectedUploadBox.UploadBoxId,
                    selectedUploadBox.TargetDirectory, ex);
                selectedUploadBox.TrySetStopMessage(
                    "Failed");
                return false;
            }
        }

        private static readonly Log
            MainReceive = Log.Tech(
            "Main receive routine: {part}/{cum} of {total} at {rate}"),
            ReceiveStop = Log.Warn(
            "Receive upload stop likely due to too much data"),
            AuxReceive = Log.Tech(
            "Trying to top up; {part}/{cum} of {total} at {rate}");

        private bool ReceiveLoop(HttpListenerRequest req, 
            IMultipartUploadParser receiveParser,
            UploadBox selectedUploadBox)
        {

            byte[] buffer;
            long totalReceiveCount = 0;
            var dataRate = new DataRateCollection(7);

            for (int readCount = req.InputStream.Read(
                buffer = receiveParser.PopForReceiving(), 0, buffer.Length);
                readCount > 0;
                readCount = req.InputStream.Read(
                buffer = receiveParser.PopForReceiving(), 0, buffer.Length))
            {
                // we report received size and data rate here
                // this might fail if ie. there was too much data
                // or some other cancellation condition was met.
                totalReceiveCount += readCount;
                if (!selectedUploadBox.TryUpdateProgress(
                    totalReceiveCount, dataRate.Add(totalReceiveCount)))
                {
                    Log.Post(ReceiveStop);
                    selectedUploadBox.TrySetStopMessage("Cancelled");
                    return false;
                }
                Log.Post(MainReceive, readCount, totalReceiveCount, 
                    selectedUploadBox.ContentLengthAnnounced,
                    selectedUploadBox.UploadBytesPerSecond); 

                // for best boundary finding at the receive parser,
                // we would like to hand in the fullest possible 
                // receive buffer. no wimpy 20 byte stuff.
                // Stream.Read doesn't guarantee that, but we can
                // Read some more.
                int fillUpCount = readCount;
                while (readCount > 0 && fillUpCount < buffer.Length)
                {
                    readCount = req.InputStream.Read(
                        buffer, fillUpCount, buffer.Length - fillUpCount);
                    fillUpCount += readCount;

                    // we report received size and data rate again, here.
                    // good for resolution and early failure when the
                    // client is giving is too much.
                    totalReceiveCount += readCount;
                    if (!selectedUploadBox.TryUpdateProgress(
                        totalReceiveCount, dataRate.Add(totalReceiveCount)))
                    {
                        selectedUploadBox.TrySetStopMessage("Cancelled");
                        return false;
                    }
                    Log.Post(AuxReceive, readCount, totalReceiveCount,
                        selectedUploadBox.ContentLengthAnnounced,
                        selectedUploadBox.UploadBytesPerSecond);
                }

                receiveParser.PushForProcessing(buffer, fillUpCount);

                // when trying to fill up we might have read an 
                // eof. then we shouldn't read again and skip
                // to finish
                if (readCount == 0) break;

                if (this.TransferRateConstraints.Sleep >= 0)
                    Thread.Sleep(this.TransferRateConstraints.Sleep);
            }
            receiveParser.Finish(selectedUploadBox.Values, selectedUploadBox.Faults);
            selectedUploadBox.TrySetStopMessage(string.Format(
                "Done with {0} errors", selectedUploadBox.Faults.Count));
            return true;
        }
    }
}
