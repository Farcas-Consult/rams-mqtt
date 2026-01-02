using System.Collections.Generic;
using ZebraIoTConnector.Persistence.Entities;

namespace ZebraIoTConnector.Persistence.Repositories
{
    public interface IStorageUnitRepository
    {
        StorageUnit? GetById(int id);
        StorageUnit? GetByName(string name);
        void Create(StorageUnit storageUnit);
    }
}
