using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using ZebraIoTConnector.Persistence.Entities;

namespace ZebraIoTConnector.Persistence.Repositories
{
    public class AssetRepository : IAssetRepository
    {
        private readonly ZebraDbContext zebraDbContext;

        public AssetRepository(ZebraDbContext zebraDbContext)
        {
            this.zebraDbContext = zebraDbContext ?? throw new ArgumentNullException(nameof(zebraDbContext));
        }

        public Asset? GetById(int id)
        {
            return zebraDbContext.Assets
                .Include(a => a.CurrentLocation)
                .FirstOrDefault(a => a.Id == id && !a.IsDeleted);
        }

        public Asset? GetByAssetNumber(string assetNumber)
        {
            if (string.IsNullOrEmpty(assetNumber))
                return null;

            return zebraDbContext.Assets
                .Include(a => a.CurrentLocation)
                .FirstOrDefault(a => a.AssetNumber == assetNumber && !a.IsDeleted);
        }

        public Asset? GetByTagIdentifier(string tagId)
        {
            if (string.IsNullOrEmpty(tagId))
                return null;

            return zebraDbContext.Assets
                .Include(a => a.CurrentLocation)
                .FirstOrDefault(a => a.TagIdentifier == tagId && !a.IsDeleted);
        }

        public IQueryable<Asset> GetAll()
        {
            return zebraDbContext.Assets
                .Include(a => a.CurrentLocation)
                .Where(a => !a.IsDeleted);
        }

        public void Create(Asset asset)
        {
            if (asset == null)
                throw new ArgumentNullException(nameof(asset));

            asset.CreatedAt = DateTime.UtcNow;
            asset.IsDeleted = false;
            zebraDbContext.Assets.Add(asset);
            zebraDbContext.SaveChanges();
        }

        public void Update(Asset asset)
        {
            if (asset == null)
                throw new ArgumentNullException(nameof(asset));

            asset.UpdatedAt = DateTime.UtcNow;
            zebraDbContext.Assets.Update(asset);
            zebraDbContext.SaveChanges();
        }

        public void Delete(int id)
        {
            var asset = GetById(id);
            if (asset != null)
            {
                asset.IsDeleted = true;
                asset.UpdatedAt = DateTime.UtcNow;
                zebraDbContext.Assets.Update(asset);
                zebraDbContext.SaveChanges();
            }
        }

        public void BulkCreate(List<Asset> assets)
        {
            if (assets == null || assets.Count == 0)
                return;

            var now = DateTime.UtcNow;
            foreach (var asset in assets)
            {
                asset.CreatedAt = now;
                asset.IsDeleted = false;
            }

            zebraDbContext.Assets.AddRange(assets);
            zebraDbContext.SaveChanges();
        }

        public List<Asset> GetAssetsNotSeenInDays(int days)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-days);
            return zebraDbContext.Assets
                .Include(a => a.CurrentLocation)
                .Where(a => !a.IsDeleted && (a.LastDiscoveredAt == null || a.LastDiscoveredAt < cutoffDate))
                .ToList();
        }

        public List<Asset> GetAssetsByLocation(int locationId)
        {
            return zebraDbContext.Assets
                .Include(a => a.CurrentLocation)
                .Where(a => !a.IsDeleted && a.CurrentLocationId == locationId)
                .ToList();
        }
    public int GetTotalCount(bool includeDeleted = false)
        {
            if (includeDeleted)
                return zebraDbContext.Assets.Count();
            return zebraDbContext.Assets.Count(a => !a.IsDeleted);
        }

        public int GetAssetsWithTagsCount()
        {
            return zebraDbContext.Assets.Count(a => !a.IsDeleted && a.TagIdentifier != null && a.TagIdentifier != "");
        }

        public int GetAssetsNotSeenInDaysCount(int days)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-days);
            return zebraDbContext.Assets.Count(a => !a.IsDeleted && (a.LastDiscoveredAt == null || a.LastDiscoveredAt < cutoffDate));
        }
    }
}



