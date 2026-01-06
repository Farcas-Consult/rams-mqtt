using ZebraIoTConnector.DomainModel.Reader;

namespace ZebraIoTConnector.Services
{
    public interface IMaterialMovementService
    {
        public Task NewTagReaded(string clientId, List<TagReadEvent> tagReadEvent);
        public Task ProcessValidMovement(ZebraIoTConnector.DomainModel.Dto.AssetMovementRequest request);
    }
}