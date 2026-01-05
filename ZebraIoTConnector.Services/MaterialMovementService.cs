using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
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
        private readonly ITagReadNotifier? tagReadNotifier;
        
        // Cooldown cache: Key = "TagId:GateId", Value = LastSeenTime
        // 30 second cooldown for gate scenarios (people walking through)
        private static readonly ConcurrentDictionary<string, DateTime> _tagCooldownCache = new();
        private static readonly TimeSpan CooldownPeriod = TimeSpan.FromSeconds(30);

        public MaterialMovementService(ILogger<MaterialMovementService> logger, IUnitOfWork unitOfWork)
            : this(logger, unitOfWork, null)
        {
        }

        // Public constructor for dependency injection with notifier
        public MaterialMovementService(ILogger<MaterialMovementService> logger, IUnitOfWork unitOfWork, ITagReadNotifier? tagReadNotifier)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            this.tagReadNotifier = tagReadNotifier;
        }

        public async Task NewTagReaded(string clientId, List<TagReadEvent> tagReadEvent)
        {
            logger.LogInformation($"[TagProcess] Processing {tagReadEvent?.Count ?? 0} tags from {clientId}");
            
            if (tagReadEvent == null || tagReadEvent.Count == 0)
            {
                logger.LogWarning("[TagProcess] No tags to process");
                return;
            }

            // Get reader entity with Gate navigation
            var reader = unitOfWork.EquipmentRepository.GetEquipmentEntityByName(clientId);
            
            if (reader == null)
            {
                logger.LogWarning($"[TagProcess] Reader {clientId} not found in database");
                return;
            }
            
            logger.LogInformation($"[TagProcess] Found reader: {reader.Name}, GateId: {reader.GateId}");

            var gate = reader.Gate;
            
            if (gate == null)
            {
                logger.LogWarning($"[TagProcess] Reader {clientId} (ID={reader.Id}) is not assigned to a gate. GateId={reader.GateId}");
                return;
            }
            
            logger.LogInformation($"[TagProcess] Gate: {gate.Name}, IsActive: {gate.IsActive}, LocationId: {gate.LocationId}");

            if (!gate.IsActive)
            {
                logger.LogWarning($"[TagProcess] Gate {gate.Name} is not active");
                return;
            }

            if (gate.Location == null)
            {
                logger.LogWarning($"[TagProcess] Gate {gate.Name} does not have a location configured");
                return;
            }
            
            logger.LogInformation($"[TagProcess] Location: {gate.Location.Name}. Processing {tagReadEvent.Count} tags...");

            // Process each tag
            foreach (var tag in tagReadEvent)
            {
                try
                {
                    // Check cooldown - skip if we've seen this tag at this gate recently
                    var cooldownKey = $"{tag.IdHex}:{gate.Id}";
                    if (_tagCooldownCache.TryGetValue(cooldownKey, out var lastSeen))
                    {
                        if (DateTime.UtcNow - lastSeen < CooldownPeriod)
                        {
                            logger.LogDebug($"Tag {tag.IdHex} at gate {gate.Name} is in cooldown, skipping");
                            continue;
                        }
                    }
                    
                    // Update cooldown timestamp
                    _tagCooldownCache[cooldownKey] = DateTime.UtcNow;
                    
                    // Cleanup old entries periodically (every 100 entries, remove stale ones)
                    if (_tagCooldownCache.Count > 100)
                    {
                        CleanupCooldownCache();
                    }
                    
                    // Look up asset by tag identifier (DON'T AUTO-CREATE!)
                    var asset = unitOfWork.AssetRepository.GetByTagIdentifier(tag.IdHex);
                    
                    if (asset == null)
                    {
                        logger.LogWarning($"Unregistered tag: {tag.IdHex} at gate {gate.Name}");
                        
                        // Send SignalR notification for unknown tag
                        if (tagReadNotifier != null)
                        {
                            try
                            {
                                var unknownMessage = new
                                {
                                    TagId = tag.IdHex,
                                    AssetNumber = (string?)null,
                                    AssetName = "Unknown Tag",
                                    Gate = gate.Name,
                                    Location = gate.Location?.Name,
                                    Timestamp = DateTime.UtcNow,
                                    Plant = (string?)null,
                                    Status = "Unknown"
                                };
                                
                                await tagReadNotifier.NotifyTagReadAsync(unknownMessage);
                                logger.LogInformation($"[LiveFeed] Unknown tag {tag.IdHex} notification sent");
                            }
                            catch (Exception ex)
                            {
                                logger.LogWarning(ex, "Failed to send unknown tag notification");
                            }
                        }
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

                    // Send SignalR message if notifier is available
                    if (tagReadNotifier != null)
                    {
                        try
                        {
                            var message = new
                            {
                                TagId = tag.IdHex,
                                AssetNumber = asset.AssetNumber,
                                AssetName = asset.Name,
                                Gate = gate.Name,
                                Location = gate.Location?.Name,
                                Timestamp = DateTime.UtcNow,
                                Plant = asset.Plant,
                                Status = "Known"
                            };
                            
                            await tagReadNotifier.NotifyTagReadAsync(message);
                        }
                        catch (Exception ex)
                        {
                            logger.LogWarning(ex, "Failed to call notifier for tag read");
                        }
                    }
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
        
        private static void CleanupCooldownCache()
        {
            var expiredKeys = _tagCooldownCache
                .Where(kvp => DateTime.UtcNow - kvp.Value > CooldownPeriod * 2)
                .Select(kvp => kvp.Key)
                .ToList();
                
            foreach (var key in expiredKeys)
            {
                _tagCooldownCache.TryRemove(key, out _);
            }
        }
    }
}
