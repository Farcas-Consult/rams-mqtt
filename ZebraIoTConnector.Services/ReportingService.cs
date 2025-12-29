using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using ZebraIoTConnector.DomainModel.Dto;
using ZebraIoTConnector.Persistence;

namespace ZebraIoTConnector.Services
{
    public class ReportingService : IReportingService
    {
        private readonly ILogger<ReportingService> logger;
        private readonly IUnitOfWork unitOfWork;

        public ReportingService(ILogger<ReportingService> logger, IUnitOfWork unitOfWork)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public List<AssetDto> GetLocationReport(LocationReportFilterDto filter)
        {
            if (filter == null)
                filter = new LocationReportFilterDto();

            var query = unitOfWork.AssetRepository.GetAll();

            if (filter.LocationId.HasValue)
                query = query.Where(a => a.CurrentLocationId == filter.LocationId.Value);

            if (!string.IsNullOrWhiteSpace(filter.Plant))
                query = query.Where(a => a.Plant == filter.Plant);

            if (!string.IsNullOrWhiteSpace(filter.CostCenter))
                query = query.Where(a => a.CostCenter == filter.CostCenter);

            var assets = query.OrderBy(a => a.CurrentLocation != null ? a.CurrentLocation.Name : "Unknown")
                             .ThenBy(a => a.AssetNumber)
                             .ToList();

            return assets.Select(a => new AssetDto
            {
                Id = a.Id,
                AssetNumber = a.AssetNumber,
                Name = a.Name,
                Description = a.Description,
                MaterialId = a.MaterialId,
                SerialNumber = a.SerialNumber,
                TechnicalId = a.TechnicalId,
                Plant = a.Plant,
                StorageLocation = a.StorageLocation,
                CostCenter = a.CostCenter,
                AssetGroup = a.AssetGroup,
                BusinessArea = a.BusinessArea,
                ObjectType = a.ObjectType,
                SystemStatus = a.SystemStatus,
                UserStatus = a.UserStatus,
                AcquisitionValue = a.AcquisitionValue,
                Comments = a.Comments,
                TagIdentifier = a.TagIdentifier,
                LastDiscoveredAt = a.LastDiscoveredAt,
                LastDiscoveredBy = a.LastDiscoveredBy,
                CurrentLocationId = a.CurrentLocationId,
                CurrentLocationName = a.CurrentLocation?.Name,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt,
                IsDeleted = a.IsDeleted
            }).ToList();
        }

        public List<AssetMovementDto> GetMovementReport(MovementReportFilterDto filter)
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            var movements = unitOfWork.AssetMovementRepository.GetMovementHistory(
                filter.From,
                filter.To,
                filter.AssetId,
                filter.GateId
            );

            // Additional filter for location if provided
            if (filter.LocationId.HasValue)
            {
                movements = movements.Where(m => 
                    m.FromLocationId == filter.LocationId.Value || 
                    m.ToLocationId == filter.LocationId.Value
                ).ToList();
            }

            return movements.Select(m => new AssetMovementDto
            {
                Id = m.Id,
                AssetId = m.AssetId,
                AssetNumber = m.Asset?.AssetNumber,
                AssetName = m.Asset?.Name,
                FromLocationId = m.FromLocationId,
                FromLocationName = m.FromLocation?.Name,
                ToLocationId = m.ToLocationId,
                ToLocationName = m.ToLocation?.Name,
                GateId = m.GateId,
                GateName = m.Gate?.Name,
                ReaderId = m.ReaderId,
                ReaderName = m.Reader?.Name,
                ReaderIdString = m.ReaderIdString,
                ReadTimestamp = m.ReadTimestamp
            }).ToList();
        }

        public List<AssetDto> GetDiscoveryReport(int daysNotSeen)
        {
            var assets = unitOfWork.AssetRepository.GetAssetsNotSeenInDays(daysNotSeen);

            return assets.Select(a => new AssetDto
            {
                Id = a.Id,
                AssetNumber = a.AssetNumber,
                Name = a.Name,
                Description = a.Description,
                MaterialId = a.MaterialId,
                SerialNumber = a.SerialNumber,
                TechnicalId = a.TechnicalId,
                Plant = a.Plant,
                StorageLocation = a.StorageLocation,
                CostCenter = a.CostCenter,
                AssetGroup = a.AssetGroup,
                BusinessArea = a.BusinessArea,
                ObjectType = a.ObjectType,
                SystemStatus = a.SystemStatus,
                UserStatus = a.UserStatus,
                AcquisitionValue = a.AcquisitionValue,
                Comments = a.Comments,
                TagIdentifier = a.TagIdentifier,
                LastDiscoveredAt = a.LastDiscoveredAt,
                LastDiscoveredBy = a.LastDiscoveredBy,
                CurrentLocationId = a.CurrentLocationId,
                CurrentLocationName = a.CurrentLocation?.Name,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt,
                IsDeleted = a.IsDeleted
            }).ToList();
        }

        public List<AssetMovementDto> GetGateActivityReport(int gateId, DateTime from, DateTime to)
        {
            var movements = unitOfWork.AssetMovementRepository.GetByGateId(gateId, from, to);

            return movements.Select(m => new AssetMovementDto
            {
                Id = m.Id,
                AssetId = m.AssetId,
                AssetNumber = m.Asset?.AssetNumber,
                AssetName = m.Asset?.Name,
                FromLocationId = m.FromLocationId,
                FromLocationName = m.FromLocation?.Name,
                ToLocationId = m.ToLocationId,
                ToLocationName = m.ToLocation?.Name,
                GateId = m.GateId,
                GateName = m.Gate?.Name,
                ReaderId = m.ReaderId,
                ReaderName = m.Reader?.Name,
                ReaderIdString = m.ReaderIdString,
                ReadTimestamp = m.ReadTimestamp
            }).ToList();
        }

        public AssetStatisticsDto GetAssetStatistics()
        {
            var allAssets = unitOfWork.AssetRepository.GetAll().ToList();
            var allGates = unitOfWork.GateRepository.GetAll();
            
            // Get total movements count (approximate from recent movements)
            // Note: This is a simple count. For exact count, we'd need a Count method in repository
            var recentMovements = unitOfWork.AssetMovementRepository.GetRecent(10000);
            var totalMovements = recentMovements.Count; // This is limited to recent 10000

            var stats = new AssetStatisticsDto
            {
                TotalAssets = allAssets.Count,
                AssetsWithTags = allAssets.Count(a => !string.IsNullOrWhiteSpace(a.TagIdentifier)),
                AssetsWithoutTags = allAssets.Count(a => string.IsNullOrWhiteSpace(a.TagIdentifier)),
                ActiveAssets = allAssets.Count(a => !a.IsDeleted),
                AssetsNotSeenIn30Days = unitOfWork.AssetRepository.GetAssetsNotSeenInDays(30).Count,
                AssetsNotSeenIn90Days = unitOfWork.AssetRepository.GetAssetsNotSeenInDays(90).Count,
                TotalGates = allGates.Count,
                ActiveGates = allGates.Count(g => g.IsActive),
                TotalMovements = totalMovements
            };

            return stats;
        }
    }
}

