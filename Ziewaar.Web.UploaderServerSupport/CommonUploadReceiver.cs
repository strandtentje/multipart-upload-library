using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Ejije.Logging;
using System.Diagnostics;
using System.IO;

namespace Ziewaar.Web.UploaderServerSupport
{
    public class CommonUploadReceiver : IUploadReceiver
    {
        private readonly IUploadSizeGovernor SizeGovernor;
        public CommonUploadReceiver(IUploadSizeGovernor sizeGovernor)
        {
            this.SizeGovernor = sizeGovernor;
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
                using (var tempFileWriter = File.OpenWrite(selectedUploadBox.TempFile))
                {
                    byte[] buffer = new byte[1024 * 16];
                    long progress = 0;
                    long currentTime = Stopwatch.GetTimestamp();
                    long lastTime = currentTime;
                    long timeDifference = 0;
                    long bytesPerTick, bytesPerSecond = 0;
                    for (int count = req.InputStream.Read(buffer, 0, buffer.Length);
                        count > 0;
                        count = req.InputStream.Read(buffer, 0, buffer.Length))
                    {
                        tempFileWriter.Write(buffer, 0, count);

                        currentTime = Stopwatch.GetTimestamp();
                        timeDifference = currentTime - lastTime;
                        lastTime = currentTime;
                        if (timeDifference > 0)
                        {
                            bytesPerTick = count / timeDifference;
                            bytesPerSecond = bytesPerTick * Stopwatch.Frequency;
                        }

                        progress += count;
                        if (!selectedUploadBox.TryUpdateProgress(
                            progress, bytesPerSecond))
                        {
                            selectedUploadBox.TrySetStopMessage(
                                "Cancelled");
                            return false;
                        }
                    }
                }
                return true;
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
    }
}
