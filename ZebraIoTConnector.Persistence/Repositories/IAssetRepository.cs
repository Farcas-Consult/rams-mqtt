using System;
using System.Collections.Generic;
using System.Linq;
using ZebraIoTConnector.Persistence.Entities;

namespace ZebraIoTConnector.Persistence.Repositories
{
    public interface IAssetRepository
    {
        Asset? GetById(int id);
        Asset? GetByAssetNumber(string assetNumber);
        Asset? GetByTagIdentifier(string tagId);
        IQueryable<Asset> GetAll();
        void Create(Asset asset);
        void Update(Asset asset);
        void Delete(int id);
        void BulkCreate(List<Asset> assets);
        List<Asset> GetAssetsNotSeenInDays(int days);
        List<Asset> GetAssetsByLocation(int locationId);
    }
}



