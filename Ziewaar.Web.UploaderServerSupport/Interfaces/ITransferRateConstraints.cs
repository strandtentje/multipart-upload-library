namespace Ziewaar.Web.UploaderServerSupport
{
    public interface ITransferRateConstraints
    {
        int Sleep { get; }
        int BufferSize { get; }
    }
}
