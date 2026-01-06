using System;
using System.Collections.Generic;

namespace ZebraIoTConnector.DomainModel.Dto
{
    public class AssetMovementRequest
    {
        public string AssetId { get; set; }
        public string ReaderId { get; set; }
        public int GateId { get; set; }
        public string Direction { get; set; } // "IN", "OUT", "UNKNOWN"
        public DateTime Timestamp { get; set; }
        
        // Metadata for logging
        public string ValidationMessage { get; set; } 
    }
}
