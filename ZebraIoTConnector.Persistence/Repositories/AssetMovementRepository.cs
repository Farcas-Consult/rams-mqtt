using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using ZebraIoTConnector.Persistence.Entities;

namespace ZebraIoTConnector.Persistence.Repositories
{
    public class AssetMovementRepository : IAssetMovementRepository
    {
        private readonly ZebraDbContext zebraDbContext;

        public AssetMovementRepository(ZebraDbContext zebraDbContext)
        {
            this.zebraDbContext = zebraDbContext ?? throw new ArgumentNullException(nameof(zebraDbContext));
        }

        public AssetMovement? GetById(int id)
        {
            return zebraDbContext.AssetMovements
                .Include(m => m.Asset)
                .Include(m => m.FromLocation)
                .Include(m => m.ToLocation)
                .Include(m => m.Gate)
                .Include(m => m.Reader)
                .FirstOrDefault(m => m.Id == id);
        }

        public IQueryable<AssetMovement> GetByAssetId(int assetId)
        {
            return zebraDbContext.AssetMovements
                .Include(m => m.Asset)
                .Include(m => m.FromLocation)
                .Include(m => m.ToLocation)
                .Include(m => m.Gate)
                .Include(m => m.Reader)
                .Where(m => m.AssetId == assetId)
                .OrderByDescending(m => m.ReadTimestamp);
        }

        public List<AssetMovement> GetByGateId(int gateId, DateTime? from, DateTime? to)
        {
            var query = zebraDbContext.AssetMovements
                .Include(m => m.Asset)
                .Include(m => m.FromLocation)
                .Include(m => m.ToLocation)
                .Include(m => m.Gate)
                .Include(m => m.Reader)
                .Where(m => m.GateId == gateId);

            if (from.HasValue)
                query = query.Where(m => m.ReadTimestamp >= from.Value);

            if (to.HasValue)
                query = query.Where(m => m.ReadTimestamp <= to.Value);

            return query
                .OrderByDescending(m => m.ReadTimestamp)
                .ToList();
        }

        public List<AssetMovement> GetRecent(int count)
        {
            return zebraDbContext.AssetMovements
                .Include(m => m.Asset)
                .Include(m => m.FromLocation)
                .Include(m => m.ToLocation)
                .Include(m => m.Gate)
                .Include(m => m.Reader)
                .OrderByDescending(m => m.ReadTimestamp)
                .Take(count)
                .ToList();
        }

        public void Create(AssetMovement movement)
        {
            if (movement == null)
                throw new ArgumentNullException(nameof(movement));

            zebraDbContext.AssetMovements.Add(movement);
            zebraDbContext.SaveChanges();
        }

        public List<AssetMovement> GetMovementHistory(DateTime from, DateTime to, int? assetId, int? gateId)
        {
            var query = zebraDbContext.AssetMovements
                .Include(m => m.Asset)
                .Include(m => m.FromLocation)
                .Include(m => m.ToLocation)
                .Include(m => m.Gate)
                .Include(m => m.Reader)
                .Where(m => m.ReadTimestamp >= from && m.ReadTimestamp <= to);

            if (assetId.HasValue)
                query = query.Where(m => m.AssetId == assetId.Value);

            if (gateId.HasValue)
                query = query.Where(m => m.GateId == gateId.Value);

            return query
                .OrderByDescending(m => m.ReadTimestamp)
                .ToList();
        }
    }
}



