using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZebraIoTConnector.DomainModel.Dto;
using ZebraIoTConnector.DomainModel.Reader;
using ZebraIoTConnector.Persistence;
using ZebraIoTConnector.Persistence.Entities;

namespace ZebraIoTConnector.Services
{
    public class MaterialMovementService : IMaterialMovementService
    {
        private readonly ILogger<MaterialMovementService> logger;
        private readonly IUnitOfWork unitOfWork;
        private readonly ITagReadNotifier? tagReadNotifier;
        private readonly ITagAggregator tagAggregator;

        public MaterialMovementService(ILogger<MaterialMovementService> logger, IUnitOfWork unitOfWork)
            : this(logger, unitOfWork, null, null)
        {
        }

        public MaterialMovementService(
            ILogger<MaterialMovementService> logger, 
            IUnitOfWork unitOfWork, 
            ITagReadNotifier? tagReadNotifier,
            ITagAggregator? tagAggregator)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            this.tagReadNotifier = tagReadNotifier;
            // Allow null for aggregator in case of legacy DI issues, but simpler to require it if we updated DI.
            // We assume DI is updated. If not, this might throw if we didn't use valid optional pattern.
            // But we registered it.
            this.tagAggregator = tagAggregator!; 
        }

        public async Task NewTagReaded(string clientId, List<TagReadEvent> tagReadEvent)
        {
            if (tagReadEvent == null || tagReadEvent.Count == 0)
                return;

            // FORCE LOG
            Console.WriteLine($"=== [TagProcess v2026.01.06] Processing {tagReadEvent.Count} tags from {clientId} ===");
            logger.LogInformation($"[TagProcess] Processing {tagReadEvent.Count} tags from {clientId}");

            var reader = unitOfWork.EquipmentRepository.GetEquipmentEntityByName(clientId);
            
            if (reader == null)
            {
                logger.LogWarning($"[TagProcess] BLOCKED: Reader '{clientId}' not registered");
                return;
            }

            var gate = reader.Gate;
            if (gate == null || !gate.IsActive || gate.Location == null)
            {
                logger.LogWarning($"[TagProcess] BLOCKED: Reader '{clientId}' Gate configuration invalid/inactive");
                return;
            }
            
            // Process each tag
            foreach (var tag in tagReadEvent)
            {
                try
                {
                    // Look up asset by tag identifier (checks both Asset.TagIdentifier and Asset.Tags collection)
                    var asset = unitOfWork.AssetRepository.GetByTagIdentifier(tag.IdHex);
                    
                    if (asset == null)
                    {
                        // Unknown Tag - Notify immediately (no aggregation needed for unknown)
                        await HandleUnknownTag(tag, gate);
                        continue;
                    }

                    // Found Asset - Push to Aggregator
                    // We need to pass the context.
                    var context = new TagReadContext
                    {
                        AssetId = asset.Id.ToString(), // Using String ID for dictionary key
                        AssetType = asset.Category ?? "Normal", // Use Category as Type (Container, Vehicle)
                        EPC = tag.IdHex,
                        AntennaId = tag.AntennaId,
                        Timestamp = DateTime.UtcNow,
                        ReaderId = clientId,
                        GateId = gate.Id
                    };

                    if (tagAggregator != null)
                    {
                        tagAggregator.AddTagRead(context);
                    }
                    else
                    {
                        // Fallback if aggregator missing (should not happen)
                        logger.LogError("TagAggregator not injected!");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Error processing tag {tag.IdHex}");
                }
            }
        }

        private async Task HandleUnknownTag(TagReadEvent tag, Gate gate)
        {
            logger.LogWarning($"Unregistered tag: {tag.IdHex} at gate {gate.Name}");
            if (tagReadNotifier != null)
            {
                try
                {
                    var unknownMessage = new
                    {
                        TagId = tag.IdHex,
                        AssetName = "Unknown Tag",
                        Gate = gate.Name,
                        Location = gate.Location?.Name,
                        Timestamp = DateTime.UtcNow,
                        Status = "Unknown"
                    };
                    await tagReadNotifier.NotifyTagReadAsync(unknownMessage);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to send unknown tag notification");
                }
            }
        }

        public async Task ProcessValidMovement(AssetMovementRequest request)
        {
            try
            {
                if (!int.TryParse(request.AssetId, out int assetIdInt))
                {
                    logger.LogError($"Invalid AssetId format: {request.AssetId}");
                    return;
                }

                var asset = unitOfWork.AssetRepository.GetById(assetIdInt);
                if (asset == null) return;

                var gate = unitOfWork.GateRepository.GetById(request.GateId);
                if (gate == null) return;

                // Update Asset Location
                var previousLocationId = asset.CurrentLocationId;
                asset.LastDiscoveredAt = request.Timestamp;
                asset.LastDiscoveredBy = $"{gate.Name} ({request.ReaderId})";
                asset.CurrentLocationId = gate.LocationId;
                asset.UpdatedAt = DateTime.UtcNow;

                // Map Direction String to Enum
                ZebraIoTConnector.DomainModel.Enums.Direction directionEnum = ZebraIoTConnector.DomainModel.Enums.Direction.None;
                if (string.Equals(request.Direction, "IN", StringComparison.OrdinalIgnoreCase)) directionEnum = ZebraIoTConnector.DomainModel.Enums.Direction.Inbound;
                if (string.Equals(request.Direction, "OUT", StringComparison.OrdinalIgnoreCase)) directionEnum = ZebraIoTConnector.DomainModel.Enums.Direction.Outbound;

                // Record Movement in DB
                var movement = new AssetMovement
                {
                    Asset = asset,
                    AssetId = asset.Id,
                    FromLocationId = previousLocationId,
                    ToLocationId = gate.LocationId,
                    GateId = gate.Id,
                    ReaderIdString = request.ReaderId,
                    ReadTimestamp = request.Timestamp,
                    Direction = directionEnum
                };
                
                // Assuming AssetMovement doesn't have Direction column yet, so just logging it
                logger.LogInformation($"[Movement] Asset {asset.AssetNumber} ({asset.Name}) moved. Direction: {request.Direction}. Validation: {request.ValidationMessage}");

                unitOfWork.AssetMovementRepository.Add(movement);
                unitOfWork.SaveChanges();

                // Send Notification
                if (tagReadNotifier != null)
                {
                    var message = new
                    {
                        TagId = asset.TagIdentifier, // Primary tag
                        AssetNumber = asset.AssetNumber,
                        AssetName = asset.Name,
                        Gate = gate.Name,
                        Location = gate.Location?.Name,
                        Timestamp = request.Timestamp,
                        Status = "Known",
                        Direction = request.Direction,
                        Message = request.ValidationMessage
                    };
                    await tagReadNotifier.NotifyTagReadAsync(message);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error processing valid movement for Asset {request.AssetId}");
            }
        }
    }
}

