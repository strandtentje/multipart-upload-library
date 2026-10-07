using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Ejije.Logging;
using System.Threading;

namespace Ziewaar.Web.UploaderServerSupport
{
    public class UploadBox : IDisposable
    {
        public readonly string SessionId, TargetDirectory, UploadBoxId, TempFile;
        public long ContentLengthAnnounced { get; private set; }
        public long ContentLengthProgress { get; private set; }
        public long UploadBytesPerSecond { get; private set; }
        public long Percentage
        {
            get
            {
                return (ContentLengthProgress * 100) / ContentLengthAnnounced;
            }
        }
        public bool IsUploading { get; private set; }
        public bool IsDisposing { get; private set; }
        public byte[] BoundaryBytes { get; private set; }
        public string StopMessage { get; private set; }

        private readonly object UploadLock = new object();
        private EventWaitHandle Stopper = new EventWaitHandle(
            false, EventResetMode.ManualReset);

        public UploadBox(string sessionId, string targetDirectory, string uploadBoxId)
        {
            this.SessionId = sessionId;
            this.TargetDirectory = targetDirectory;
            this.UploadBoxId = uploadBoxId;
            this.TempFile = Path.GetTempFileName();
        }

        public bool TrySetStopMessage(string message)
        {
            lock (UploadLock)
            {
                if (StopMessage != null || message == null) return false;
                Stopper.Set();
                StopMessage = message;
                return true;
            }
        }

        public bool TryWaitForFinish(out string message)
        {
            bool wasStopped = false;
            try
            {
                wasStopped = Stopper.WaitOne(924);
            }
            catch (Exception ex)
            {
                wasStopped = true;
                // huh.
            }
            lock (UploadLock)
            {
                if (StopMessage != null)
                {
                    message = StopMessage;
                }
                else if (Stopper == null)
                {
                    message = "Stopped for unknown reasons";
                    wasStopped = true;
                }
                else
                {
                    message = "Working";
                }
                return wasStopped;
            }
        }

        public bool ValidateUntainted()
        {
            lock (UploadLock)
            {
                return !IsUploading && !IsDisposing && StopMessage == null;
            }
        }

        public bool TryInitiateUploading(long announcedLength, string boundary)
        {
            lock (UploadLock)
            {
                if (IsUploading || IsDisposing || StopMessage != null) return false;
                IsUploading = true;
                this.ContentLengthAnnounced = announcedLength;
                this.BoundaryBytes = Encoding.ASCII.GetBytes(boundary);
                return true;
            }
        }

        public bool TryUpdateProgress(long progress, long bytesPerSecond)
        {
            lock (UploadLock)
            {
                if (!IsUploading || IsDisposing || StopMessage != null) return false;
                if (progress > ContentLengthAnnounced)
                {
                    StopMessage = "File huge";
                    return false;
                }
                this.ContentLengthProgress = progress;
                this.UploadBytesPerSecond = bytesPerSecond;
                return true;
            }
        }

        public static UploadBox CreateFresh(string sessionId, string path)
        {
            return new UploadBox(
                sessionId, path,
                Guid.NewGuid().ToString());
        }
        public static UploadBox CreateFinder(string sessionId, string uploadBoxId)
        {
            return new UploadBox(sessionId, string.Empty, uploadBoxId);
        }

        private static readonly Log
            FailedToDelete = Log.Fail("Failed to delete {tempfile} {dueto}");

        public void Dispose()
        {
            try
            {
                lock (UploadLock)
                {
                    if (IsDisposing) return;
                    IsDisposing = true;
                    if (File.Exists(TempFile))
                        File.Delete(TempFile);
                }
            }
            catch (Exception ex)
            {
                Log.Post(FailedToDelete, TempFile, ex);
            }
            finally
            {
                try
                {
                    lock (UploadLock)
                    {
                        this.Stopper.Close();
                        this.Stopper = null;
                    }
                }
                catch (Exception ex)
                {
                    // ok
                }
            }
        }
    }
}
