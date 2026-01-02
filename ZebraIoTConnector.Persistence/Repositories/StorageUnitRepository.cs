using System;
using System.Linq;
using ZebraIoTConnector.Persistence.Entities;

namespace ZebraIoTConnector.Persistence.Repositories
{
    public class StorageUnitRepository : IStorageUnitRepository
    {
        private readonly ZebraDbContext zebraDbContext;

        public StorageUnitRepository(ZebraDbContext zebraDbContext)
        {
            this.zebraDbContext = zebraDbContext ?? throw new ArgumentNullException(nameof(zebraDbContext));
        }

        public StorageUnit? GetById(int id)
        {
            return zebraDbContext.StorageUnits.FirstOrDefault(s => s.Id == id);
        }

        public StorageUnit? GetByName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;
            return zebraDbContext.StorageUnits.FirstOrDefault(s => s.Name == name);
        }

        public void Create(StorageUnit storageUnit)
        {
            if (storageUnit == null)
                throw new ArgumentNullException(nameof(storageUnit));

            zebraDbContext.StorageUnits.Add(storageUnit);
            zebraDbContext.SaveChanges();
        }
    }
}
