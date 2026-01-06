using ZebraIoTConnector.DomainModel.Reader;

namespace ZebraIoTConnector.FXReaderInterface
{
    public interface IFXReaderManager
    {
        public void HearthBeatEventReceived(HeartBeatEvent heartBeatEvent);
        public Task TagDataEventReceivedAsync(string clientId, List<TagReadEvent> tagReadEvent);
        public void GPInStatusChanged();
        List<string> GetReaderNames();
    }
}