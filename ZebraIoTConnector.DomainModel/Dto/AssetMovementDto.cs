using System;

namespace ZebraIoTConnector.DomainModel.Dto
{
    public class AssetMovementDto
    {
        public int Id { get; set; }
        public int AssetId { get; set; }
        public string? AssetNumber { get; set; }
        public string? AssetName { get; set; }
        public int? FromLocationId { get; set; }
        public string? FromLocationName { get; set; }
        public int ToLocationId { get; set; }
        public string? ToLocationName { get; set; }
        public int? GateId { get; set; }
        public string? GateName { get; set; }
        public int? ReaderId { get; set; }
        public string? ReaderName { get; set; }
        public string? ReaderIdString { get; set; }
        public DateTime ReadTimestamp { get; set; }
    }
}



