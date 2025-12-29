using System;
using System.Collections.Generic;
using System.Linq;
using ZebraIoTConnector.Persistence.Entities;

namespace ZebraIoTConnector.Persistence.Repositories
{
    public interface IAssetMovementRepository
    {
        AssetMovement? GetById(int id);
        IQueryable<AssetMovement> GetByAssetId(int assetId);
        List<AssetMovement> GetByGateId(int gateId, DateTime? from, DateTime? to);
        List<AssetMovement> GetRecent(int count);
        void Create(AssetMovement movement);
        List<AssetMovement> GetMovementHistory(DateTime from, DateTime to, int? assetId, int? gateId);
    }
}



