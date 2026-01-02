using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using ZebraIoTConnector.DomainModel.Enums;

namespace ZebraIoTConnector.Persistence.Entities
{
    [Index(nameof(AssetId))]
    [Index(nameof(ReadTimestamp))]
    [Index(nameof(GateId))]
    public class AssetMovement
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int AssetId { get; set; }
        public int? FromLocationId { get; set; }
        public int ToLocationId { get; set; }
        public int? GateId { get; set; }
        public int? ReaderId { get; set; }
        public string? ReaderIdString { get; set; }
        public DateTime ReadTimestamp { get; set; }
        public ZebraIoTConnector.DomainModel.Enums.Direction Direction { get; set; }

        // Navigation properties
        public Asset Asset { get; set; }
        public StorageUnit? FromLocation { get; set; }
        public StorageUnit ToLocation { get; set; }
        public Gate? Gate { get; set; }
        public Equipment? Reader { get; set; }
    }
}



