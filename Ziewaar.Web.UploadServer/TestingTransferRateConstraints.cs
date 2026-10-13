using Ziewaar.Web.UploaderServerSupport;

namespace Ziewaar.Web.UploadServer
{
    internal class TestingTransferRateConstraints : ITransferRateConstraints
    {
        public int Sleep => -1;
        public int BufferSize => 64 * 1024;
    }
}