using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ZebraIoTConnector.DomainModel.Dto;

namespace ZebraIoTConnector.Services
{
    public class TagAggregator : ITagAggregator
    {
        private readonly ILogger<TagAggregator> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ConcurrentDictionary<string, AssetBuffer> _buffers = new ConcurrentDictionary<string, AssetBuffer>();
        
        // BUFFER WINDOW: 2 seconds of silence required to trigger processing
        private const int BUFFER_WINDOW_MS = 2000;

        public TagAggregator(ILogger<TagAggregator> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        public void AddTagRead(TagReadContext context)
        {
            // Use AssetId as key (grouping all tags for the same asset)
            var buffer = _buffers.GetOrAdd(context.AssetId, id => new AssetBuffer(id, context, ProcessBuffer));

            buffer.AddRead(context);
        }

        private async void ProcessBuffer(AssetBuffer buffer)
        {
            try
            {
                // Remove from dictionary so new reads start a new buffer (after this one is processed)
                _buffers.TryRemove(buffer.AssetId, out _);

                var reads = buffer.GetReads();
                if (reads.Count == 0) return;

                var context = buffer.InitialContext; // Use initial context for static data (Reader, Gate, AssetType)
                
                // 1. Validate Completeness
                var uniqueTags = reads.Select(r => r.EPC).Distinct().ToList();
                bool isValid = ValidateAssetCompleteness(context.AssetType, uniqueTags.Count, out string validationMsg);

                if (!isValid)
                {
                    _logger.LogWarning($"[TagAggregator] Incomplete read for {context.AssetType} {context.AssetId}. {validationMsg}");
                    // Optionally report "Partial" or just "Unknown"? For now, identifying "Incomplete" is enough.
                    // We might still want to report it, but with a warning.
                    // Let's Proceed but mark as "Partial".
                }

                // 2. Determine Direction
                // Sequence logic: First Antenna -> Last Antenna
                var firstRead = reads.OrderBy(r => r.Timestamp).First();
                var lastRead = reads.OrderBy(r => r.Timestamp).Last();

                string direction = "UNKNOWN";

                if (firstRead.AntennaId != lastRead.AntennaId)
                {
                    if (firstRead.AntennaId == 2 && lastRead.AntennaId == 1)
                    {
                        direction = "OUT";
                    }
                    else if (firstRead.AntennaId == 1 && lastRead.AntennaId == 2)
                    {
                        direction = "IN";
                    }
                    else
                    {
                        // Generic case for other ports
                        direction = $"{firstRead.AntennaId}->{lastRead.AntennaId}";
                    }
                }
                else
                {
                    // Stationary or single read
                    // Use single antenna location if known? 
                    // For now, default to "IN" if Antenna 1 (Legacy behavior) or keep UNKNOWN?
                    // User only specified sequence logic.
                    // Let's fallback to "IN" if only Antenna 1 seen (assuming it's entry).
                    if (firstRead.AntennaId == 1) direction = "IN"; // Legacy fallback
                    if (firstRead.AntennaId == 2) direction = "OUT"; // Legacy fallback
                }

                _logger.LogInformation($"[TagAggregator] Asset {context.AssetId} ({context.AssetType}) processed. Tags: {uniqueTags.Count}. Direction: {direction}. ({validationMsg})");

                // 3. Dispatch to MaterialMovementService
                using (var scope = _scopeFactory.CreateScope())
                {
                    var service = scope.ServiceProvider.GetRequiredService<IMaterialMovementService>();
                    
                    var request = new AssetMovementRequest
                    {
                        AssetId = context.AssetId,
                        ReaderId = context.ReaderId,
                        GateId = context.GateId,
                        Direction = direction,
                        Timestamp = DateTime.UtcNow,
                        ValidationMessage = validationMsg
                    };

                    await service.ProcessValidMovement(request);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[TagAggregator] Error processing buffer for Asset {buffer.AssetId}");
            }
        }

        private bool ValidateAssetCompleteness(string assetType, int tagCount, out string message)
        {
            message = "OK";
            assetType = assetType?.ToLower() ?? "normal";

            if (assetType.Contains("container"))
            {
                if (tagCount < 3)
                {
                    message = $"Expected 3 tags for Container, found {tagCount}";
                    return false;
                }
            }
            else if (assetType.Contains("vehicle"))
            {
                if (tagCount < 2)
                {
                    message = $"Expected 2 tags for Vehicle, found {tagCount}";
                    return false;
                }
            }
            else
            {
                // Normal
                if (tagCount < 1)
                {
                     message = "No tags found";
                     return false;
                }
            }
            return true;
        }

        private class AssetBuffer
        {
            public string AssetId { get; }
            public TagReadContext InitialContext { get; }
            
            private readonly List<TagReadContext> _reads = new List<TagReadContext>();
            private readonly object _lock = new object();
            private readonly Timer _timer;
            private readonly Action<AssetBuffer> _callback;

            public AssetBuffer(string assetId, TagReadContext context, Action<AssetBuffer> callback)
            {
                AssetId = assetId;
                InitialContext = context;
                _callback = callback;
                
                // Initialize timer but don't start it yet (AddRead will start it)
                _timer = new Timer(OnTimerCallback, null, Timeout.Infinite, Timeout.Infinite);
            }

            public void AddRead(TagReadContext context)
            {
                lock (_lock)
                {
                    _reads.Add(context);
                }
                // Reset timer (sliding window)
                _timer.Change(BUFFER_WINDOW_MS, Timeout.Infinite);
            }

            public List<TagReadContext> GetReads()
            {
                lock (_lock)
                {
                    return new List<TagReadContext>(_reads);
                }
            }

            private void OnTimerCallback(object state)
            {
                _callback(this);
            }
        }
    }
}
