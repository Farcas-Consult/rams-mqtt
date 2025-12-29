using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using ZebraIoTConnector.DomainModel.Reader;
using ZebraIoTConnector.Persistence;
using ZebraIoTConnector.Persistence.Entities;

namespace ZebraIoTConnector.Services
{
    public class MaterialMovementService : IMaterialMovementService
    {
        private readonly ILogger<MaterialMovementService> logger;
        private readonly IUnitOfWork unitOfWork;

        public MaterialMovementService(ILogger<MaterialMovementService> logger, IUnitOfWork unitOfWork)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public void NewTagReaded(string clientId, List<TagReadEvent> tagReadEvent)
        {
            if (tagReadEvent == null || tagReadEvent.Count == 0)
                return;

            // Get reader entity with Gate navigation
            var reader = unitOfWork.EquipmentRepository.GetEquipmentEntityByName(clientId);
            
            if (reader == null)
            {
                logger.LogWarning($"Reader {clientId} not registered yet, tag read message received before the heartbeat");
                return;
            }

            var gate = reader.Gate;
            
            if (gate == null)
            {
                logger.LogWarning($"Reader {clientId} is not assigned to a gate");
                return;
            }

            if (!gate.IsActive)
            {
                logger.LogWarning($"Gate {gate.Name} is not active");
                return;
            }

            if (gate.Location == null)
            {
                logger.LogWarning($"Gate {gate.Name} does not have a location configured");
                return;
            }

            // Process each tag
            foreach (var tag in tagReadEvent)
            {
                try
                {
                    // Look up asset by tag identifier (DON'T AUTO-CREATE!)
                    var asset = unitOfWork.AssetRepository.GetByTagIdentifier(tag.IdHex);
                    
                    if (asset == null)
                    {
                        logger.LogWarning($"Unregistered tag: {tag.IdHex} at gate {gate.Name}");
                        // Optionally: Push to SignalR for unregistered tag alert
                        continue;
                    }

                    // Store previous location before updating
                    var previousLocation = asset.CurrentLocation;
                    var previousLocationId = asset.CurrentLocationId;

                    // Update asset tracking fields
                    asset.LastDiscoveredAt = DateTime.UtcNow;
                    asset.LastDiscoveredBy = gate.Name;
                    asset.CurrentLocationId = gate.Location.Id;
                    asset.UpdatedAt = DateTime.UtcNow;

                    // Record movement
                    var movement = new AssetMovement
                    {
                        Asset = asset,
                        AssetId = asset.Id,
                        FromLocationId = previousLocationId,
                        ToLocationId = gate.Location.Id,
                        GateId = gate.Id,
                        ReaderId = reader.Id,
                        ReaderIdString = clientId,
                        ReadTimestamp = DateTime.UtcNow
                    };

                    unitOfWork.AssetMovementRepository.Add(movement);

                    logger.LogInformation($"Asset {asset.AssetNumber} ({asset.Name}) detected at gate {gate.Name}");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Error processing tag {tag.IdHex} at gate {gate.Name}");
                }
            }

            // Save all changes
            try
            {
                unitOfWork.SaveChanges();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error saving asset movements for reader {clientId}");
                throw;
            }
        }
    }
}
