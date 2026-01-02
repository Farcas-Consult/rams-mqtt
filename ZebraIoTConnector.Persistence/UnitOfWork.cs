using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZebraIoTConnector.Persistence.Repositories;

namespace ZebraIoTConnector.Persistence
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly ZebraDbContext zebraDbContext;
        
        private readonly EquipmentRepository equipmentRepository;
        private readonly SublotRepository sublotRepository;
        private readonly InventoryOperationRepository inventoryOperationRepository;

        // Asset tracking repositories
        // Asset tracking repositories
        private readonly AssetRepository assetRepository;
        private readonly GateRepository gateRepository;
        private readonly AssetMovementRepository assetMovementRepository;
        private readonly SiteRepository siteRepository;
        private readonly StorageUnitRepository storageUnitRepository;

        public IEquipmentRepository EquipmentRepository => equipmentRepository;
        public ISublotRepository SublotRepository => sublotRepository;
        public IInventoryOperationRepository InventoryOperationRepository => inventoryOperationRepository;

        // Asset tracking repositories
        public IAssetRepository AssetRepository => assetRepository;
        public IGateRepository GateRepository => gateRepository;
        public IAssetMovementRepository AssetMovementRepository => assetMovementRepository;
        public ISiteRepository SiteRepository => siteRepository;
        public IStorageUnitRepository StorageUnitRepository => storageUnitRepository;

        public UnitOfWork(ZebraDbContext zebraDbContext)
        {
            this.zebraDbContext = zebraDbContext ?? throw new ArgumentNullException(nameof(zebraDbContext));

            equipmentRepository = new EquipmentRepository(zebraDbContext);
            sublotRepository = new SublotRepository(zebraDbContext);
            inventoryOperationRepository = new InventoryOperationRepository(zebraDbContext);

            // Asset tracking repositories
            assetRepository = new AssetRepository(zebraDbContext);
            gateRepository = new GateRepository(zebraDbContext);
            assetMovementRepository = new AssetMovementRepository(zebraDbContext);
            siteRepository = new SiteRepository(zebraDbContext);
            storageUnitRepository = new StorageUnitRepository(zebraDbContext);
        }

        public void BeginTransaction()
        {
            zebraDbContext.Database.BeginTransaction();
        }

        public void CommitTransaction()
        {
            zebraDbContext.Database.CommitTransaction();
        }

        public void RollbackTransaction()
        {
            zebraDbContext.Database.RollbackTransaction();
        }

        public int SaveChanges()
        {
            return zebraDbContext.SaveChanges();
        }

        public void Dispose()
        {
            zebraDbContext.Dispose();
        }
    }
}
