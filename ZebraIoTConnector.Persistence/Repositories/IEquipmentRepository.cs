using ZebraIoTConnector.DomainModel.Dto;
using ZebraIoTConnector.Persistence.Entities;

namespace ZebraIoTConnector.Persistence.Repositories
{
    public interface IEquipmentRepository
    {
        void AddIfNotExists(string equipmentName, string storageUnitName);
        List<EquipmentDto> GetEquipments();
        EquipmentDto GetEquipmentByName(string name);
        Equipment? GetEquipmentEntityByName(string name);
        Equipment? GetEquipmentById(int id);
    }
}