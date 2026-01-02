using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using ZebraIoTConnector.DomainModel.Dto;
using ZebraIoTConnector.Persistence;
using ZebraIoTConnector.Persistence.Entities;

namespace ZebraIoTConnector.Services
{
    public class AssetManagementService : IAssetManagementService
    {
        private readonly ILogger<AssetManagementService> logger;
        private readonly IUnitOfWork unitOfWork;

        public AssetManagementService(ILogger<AssetManagementService> logger, IUnitOfWork unitOfWork)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public AssetDto CreateAsset(CreateAssetDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (string.IsNullOrWhiteSpace(dto.AssetNumber))
                throw new ArgumentException("AssetNumber is required", nameof(dto));

            // Check for duplicate AssetNumber
            var existingAsset = unitOfWork.AssetRepository.GetByAssetNumber(dto.AssetNumber);
            if (existingAsset != null)
                throw new InvalidOperationException($"Asset with AssetNumber '{dto.AssetNumber}' already exists");

            // Check for duplicate TagIdentifier if provided
            if (!string.IsNullOrWhiteSpace(dto.TagIdentifier))
            {
                var existingTag = unitOfWork.AssetRepository.GetByTagIdentifier(dto.TagIdentifier);
                if (existingTag != null)
                    throw new InvalidOperationException($"Tag '{dto.TagIdentifier}' is already assigned to another asset");
            }

            var asset = new Asset
            {
                AssetNumber = dto.AssetNumber,
                Name = dto.Name,
                Description = dto.Description,
                MaterialId = dto.MaterialId,
                SerialNumber = dto.SerialNumber,
                TechnicalId = dto.TechnicalId,
                Plant = dto.Plant,
                StorageLocation = dto.StorageLocation,
                CostCenter = dto.CostCenter,
                AssetGroup = dto.AssetGroup,
                BusinessArea = dto.BusinessArea,
                ObjectType = dto.ObjectType,
                SystemStatus = dto.SystemStatus,
                UserStatus = dto.UserStatus,
                AcquisitionValue = dto.AcquisitionValue,
                Comments = dto.Comments,
                Manufacturer = dto.Manufacturer,
                PurchaseDate = dto.PurchaseDate,
                Category = dto.Category,
                Location = dto.Location,
                TagIdentifier = dto.TagIdentifier,
                CurrentLocationId = dto.CurrentLocationId,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            unitOfWork.AssetRepository.Create(asset);

            return MapToDto(asset);
        }

        public AssetDto UpdateAsset(int id, UpdateAssetDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var asset = unitOfWork.AssetRepository.GetById(id);
            if (asset == null)
                throw new InvalidOperationException($"Asset with ID {id} not found");

            // Check for duplicate TagIdentifier if being changed
            if (!string.IsNullOrWhiteSpace(dto.TagIdentifier) && dto.TagIdentifier != asset.TagIdentifier)
            {
                var existingTag = unitOfWork.AssetRepository.GetByTagIdentifier(dto.TagIdentifier);
                if (existingTag != null && existingTag.Id != id)
                    throw new InvalidOperationException($"Tag '{dto.TagIdentifier}' is already assigned to another asset");
            }

            // Update fields
            asset.Name = dto.Name ?? asset.Name;
            asset.Description = dto.Description ?? asset.Description;
            asset.MaterialId = dto.MaterialId ?? asset.MaterialId;
            asset.SerialNumber = dto.SerialNumber ?? asset.SerialNumber;
            asset.TechnicalId = dto.TechnicalId ?? asset.TechnicalId;
            asset.Plant = dto.Plant ?? asset.Plant;
            asset.StorageLocation = dto.StorageLocation ?? asset.StorageLocation;
            asset.CostCenter = dto.CostCenter ?? asset.CostCenter;
            asset.AssetGroup = dto.AssetGroup ?? asset.AssetGroup;
            asset.BusinessArea = dto.BusinessArea ?? asset.BusinessArea;
            asset.ObjectType = dto.ObjectType ?? asset.ObjectType;
            asset.SystemStatus = dto.SystemStatus ?? asset.SystemStatus;
            asset.UserStatus = dto.UserStatus ?? asset.UserStatus;
            asset.AcquisitionValue = dto.AcquisitionValue ?? asset.AcquisitionValue;
            asset.Comments = dto.Comments ?? asset.Comments;

            asset.Manufacturer = dto.Manufacturer ?? asset.Manufacturer;
            asset.PurchaseDate = dto.PurchaseDate ?? asset.PurchaseDate;
            asset.Category = dto.Category ?? asset.Category;
            asset.Location = dto.Location ?? asset.Location;
            
            if (dto.TagIdentifier != null)
                asset.TagIdentifier = dto.TagIdentifier;
            
            if (dto.CurrentLocationId.HasValue)
                asset.CurrentLocationId = dto.CurrentLocationId;

            asset.UpdatedAt = DateTime.UtcNow;

            unitOfWork.AssetRepository.Update(asset);

            return MapToDto(asset);
        }

        public AssetDto? GetAsset(int id)
        {
            var asset = unitOfWork.AssetRepository.GetById(id);
            return asset != null ? MapToDto(asset) : null;
        }

        public AssetDto? GetAssetByTag(string tagIdentifier)
        {
            if (string.IsNullOrWhiteSpace(tagIdentifier))
                return null;

            var asset = unitOfWork.AssetRepository.GetByTagIdentifier(tagIdentifier);
            return asset != null ? MapToDto(asset) : null;
        }

        public (List<AssetDto> Assets, int TotalCount) GetAssets(AssetFilterDto filter)
        {
            if (filter == null)
                filter = new AssetFilterDto();

            var query = unitOfWork.AssetRepository.GetAll();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(filter.Plant))
                query = query.Where(a => a.Plant == filter.Plant);

            if (!string.IsNullOrWhiteSpace(filter.CostCenter))
                query = query.Where(a => a.CostCenter == filter.CostCenter);

            if (filter.LocationId.HasValue)
                query = query.Where(a => a.CurrentLocationId == filter.LocationId.Value);

            if (!string.IsNullOrWhiteSpace(filter.AssetNumber))
                query = query.Where(a => a.AssetNumber.Contains(filter.AssetNumber));

            if (!string.IsNullOrWhiteSpace(filter.Name))
                query = query.Where(a => a.Name.Contains(filter.Name));

            if (!string.IsNullOrWhiteSpace(filter.TagIdentifier))
                query = query.Where(a => a.TagIdentifier == filter.TagIdentifier);

            if (filter.DaysNotSeen.HasValue)
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-filter.DaysNotSeen.Value);
                query = query.Where(a => a.LastDiscoveredAt == null || a.LastDiscoveredAt < cutoffDate);
            }

            if (filter.IsDeleted.HasValue)
            {
                if (filter.IsDeleted.Value)
                    query = query.Where(a => a.IsDeleted);
                else
                    query = query.Where(a => !a.IsDeleted);
            }

            var totalCount = query.Count();

            // Apply pagination
            var assets = query
                .OrderBy(a => a.AssetNumber)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();

            var assetDtos = assets.Select(MapToDto).ToList();

            return (assetDtos, totalCount);
        }

        public void DeleteAsset(int id)
        {
            var asset = unitOfWork.AssetRepository.GetById(id);
            if (asset == null)
                throw new InvalidOperationException($"Asset with ID {id} not found");

            unitOfWork.AssetRepository.Delete(id);
        }

        public void AssignTagToAsset(int assetId, string tagIdentifier)
        {
            if (string.IsNullOrWhiteSpace(tagIdentifier))
                throw new ArgumentException("TagIdentifier is required", nameof(tagIdentifier));

            var asset = unitOfWork.AssetRepository.GetById(assetId);
            if (asset == null)
                throw new InvalidOperationException($"Asset with ID {assetId} not found");

            // Check if tag is already assigned to another asset
            var existingAsset = unitOfWork.AssetRepository.GetByTagIdentifier(tagIdentifier);
            if (existingAsset != null && existingAsset.Id != assetId)
                throw new InvalidOperationException($"Tag '{tagIdentifier}' is already assigned to asset '{existingAsset.AssetNumber}'");

            asset.TagIdentifier = tagIdentifier;
            asset.UpdatedAt = DateTime.UtcNow;

            unitOfWork.AssetRepository.Update(asset);
        }

        public void UnassignTag(int assetId)
        {
            var asset = unitOfWork.AssetRepository.GetById(assetId);
            if (asset == null)
                throw new InvalidOperationException($"Asset with ID {assetId} not found");

            asset.TagIdentifier = null;
            asset.UpdatedAt = DateTime.UtcNow;

            unitOfWork.AssetRepository.Update(asset);
        }

        public AssetImportResultDto BulkImportAssets(List<BulkImportAssetDto> assets)
        {
            if (assets == null || assets.Count == 0)
                return new AssetImportResultDto
                {
                    Success = true,
                    TotalRows = 0,
                    SuccessCount = 0,
                    ErrorCount = 0
                };

            var result = new AssetImportResultDto
            {
                TotalRows = assets.Count,
                Errors = new List<AssetImportErrorDto>()
            };

            var validAssets = new List<Asset>();
            var assetNumbers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < assets.Count; i++)
            {
                var rowNumber = i + 1;
                var importDto = assets[i];

                try
                {
                    // Validate required fields
                    if (string.IsNullOrWhiteSpace(importDto.Equipment))
                    {
                        result.Errors.Add(new AssetImportErrorDto
                        {
                            RowNumber = rowNumber,
                            AssetNumber = null,
                            Error = "AssetNumber (Equipment) is required"
                        });
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(importDto.Description))
                    {
                        result.Errors.Add(new AssetImportErrorDto
                        {
                            RowNumber = rowNumber,
                            AssetNumber = importDto.Equipment,
                            Error = "Name (Description) is required"
                        });
                        continue;
                    }

                    // Check for duplicate in import batch
                    if (assetNumbers.Contains(importDto.Equipment))
                    {
                        result.Errors.Add(new AssetImportErrorDto
                        {
                            RowNumber = rowNumber,
                            AssetNumber = importDto.Equipment,
                            Error = $"Duplicate AssetNumber in import batch"
                        });
                        continue;
                    }

                    // Check for duplicate in database
                    var existingAsset = unitOfWork.AssetRepository.GetByAssetNumber(importDto.Equipment);
                    if (existingAsset != null)
                    {
                        result.Errors.Add(new AssetImportErrorDto
                        {
                            RowNumber = rowNumber,
                            AssetNumber = importDto.Equipment,
                            Error = $"AssetNumber already exists in database"
                        });
                        continue;
                    }

                    var asset = new Asset
                    {
                        AssetNumber = importDto.Equipment,
                        Name = importDto.Description,
                        Description = importDto.MaterialDescription,
                        MaterialId = importDto.Material,
                        SerialNumber = importDto.ManufSerialNumber,
                        TechnicalId = importDto.TechIdentNo,
                        Plant = importDto.Plnt,
                        StorageLocation = importDto.SLoc,
                        CostCenter = importDto.CostCtr,
                        AssetGroup = importDto.AGrp,
                        BusinessArea = importDto.BusA,
                        ObjectType = importDto.ObjectType,
                        SystemStatus = importDto.SysStatus,
                        UserStatus = importDto.UserStatus,
                        AcquisitionValue = importDto.AcquistnValue,
                        Comments = importDto.Comment,
                        Manufacturer = importDto.Manufacturer,
                        PurchaseDate = importDto.PurchaseDate,
                        Category = importDto.Category,
                        Location = importDto.Location,
                        CreatedAt = DateTime.UtcNow,
                        IsDeleted = false
                    };

                    validAssets.Add(asset);
                    assetNumbers.Add(importDto.Equipment);
                }
                catch (Exception ex)
                {
                    result.Errors.Add(new AssetImportErrorDto
                    {
                        RowNumber = rowNumber,
                        AssetNumber = importDto.Equipment,
                        Error = $"Error processing row: {ex.Message}"
                    });
                }
            }

            // Bulk create valid assets
            if (validAssets.Count > 0)
            {
                try
                {
                    unitOfWork.AssetRepository.BulkCreate(validAssets);
                    result.SuccessCount = validAssets.Count;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error during bulk asset creation");
                    result.Errors.Add(new AssetImportErrorDto
                    {
                        RowNumber = 0,
                        AssetNumber = null,
                        Error = $"Bulk creation failed: {ex.Message}"
                    });
                }
            }

            result.ErrorCount = result.Errors.Count;
            result.Success = result.ErrorCount == 0;

            return result;
        }

        public void ReportMovement(ReportMovementDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (string.IsNullOrWhiteSpace(dto.TagId))
                throw new ArgumentException("TagId is required", nameof(dto));

            if (string.IsNullOrWhiteSpace(dto.DeviceId))
                throw new ArgumentException("DeviceId is required", nameof(dto));

            // 1. Get Asset
            var asset = unitOfWork.AssetRepository.GetByTagIdentifier(dto.TagId);
            if (asset == null)
            {
                logger.LogWarning($"Unregistered tag {dto.TagId} reported by mobile device {dto.DeviceId}");
                // Optionally auto-create or just log
                return;
            }

            // 2. Get Gate/Location
            Gate gate = null;
            if (dto.GateId > 0)
            {
                gate = unitOfWork.GateRepository.GetById(dto.GateId);
                if (gate == null)
                {
                    logger.LogWarning($"Invalid GateId {dto.GateId} reported by mobile device {dto.DeviceId}");
                    return; 
                }

                if (!gate.LocationId.HasValue)
                {
                    logger.LogWarning($"Gate {gate.Name} (Id: {gate.Id}) does not have a configured LocationId");
                    throw new InvalidOperationException($"Gate {gate.Name} does not have a configured Location");
                }
            }

            // 3. Update Asset
            var previousLocationId = asset.CurrentLocationId;
            asset.LastDiscoveredAt = dto.Timestamp;
            asset.LastDiscoveredBy = $"Mobile: {dto.DeviceId}";
            
            if (gate != null && gate.LocationId.HasValue)
            {
                asset.CurrentLocationId = gate.LocationId;
            }

            asset.UpdatedAt = DateTime.UtcNow;

            // 4. Record Movement
            var movement = new AssetMovement
            {
                AssetId = asset.Id,
                Asset = asset,
                FromLocationId = previousLocationId,
                ToLocationId = asset.CurrentLocationId ?? 0, // Fallback if no location
                GateId = gate?.Id,
                ReaderId = null, // No registered reader entity
                ReaderIdString = dto.DeviceId,
                ReadTimestamp = dto.Timestamp
            };

            unitOfWork.AssetMovementRepository.Add(movement);
            unitOfWork.AssetRepository.Update(asset);
            unitOfWork.SaveChanges();

            logger.LogInformation($"Asset {asset.AssetNumber} reported at {(gate?.Name ?? "Unknown Gate")} by device {dto.DeviceId}");
        }

        private AssetDto MapToDto(Asset asset)
        {
            if (asset == null)
                return null;
            
            // ... existing mapping ...
            return new AssetDto
            {
                Id = asset.Id,
                AssetNumber = asset.AssetNumber,
                Name = asset.Name,
                // ... map all fields ...
                Description = asset.Description,
                MaterialId = asset.MaterialId,
                SerialNumber = asset.SerialNumber,
                TechnicalId = asset.TechnicalId,
                Plant = asset.Plant,
                StorageLocation = asset.StorageLocation,
                CostCenter = asset.CostCenter,
                AssetGroup = asset.AssetGroup,
                BusinessArea = asset.BusinessArea,
                ObjectType = asset.ObjectType,
                SystemStatus = asset.SystemStatus,
                UserStatus = asset.UserStatus,
                AcquisitionValue = asset.AcquisitionValue,
                Comments = asset.Comments,
                Manufacturer = asset.Manufacturer,
                PurchaseDate = asset.PurchaseDate,
                Category = asset.Category,
                Location = asset.Location,
                TagIdentifier = asset.TagIdentifier,
                LastDiscoveredAt = asset.LastDiscoveredAt,
                LastDiscoveredBy = asset.LastDiscoveredBy,
                CurrentLocationId = asset.CurrentLocationId,
                CurrentLocationName = asset.CurrentLocation?.Name,
                CreatedAt = asset.CreatedAt,
                UpdatedAt = asset.UpdatedAt,
                IsDeleted = asset.IsDeleted
            };
        }
    }
}

