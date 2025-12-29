using System;

namespace ZebraIoTConnector.DomainModel.Dto
{
    public class AssetDto
    {
        public int Id { get; set; }
        public string AssetNumber { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }

        // SAP fields
        public string? MaterialId { get; set; }
        public string? SerialNumber { get; set; }
        public string? TechnicalId { get; set; }
        public string? Plant { get; set; }
        public string? StorageLocation { get; set; }
        public string? CostCenter { get; set; }
        public string? AssetGroup { get; set; }
        public string? BusinessArea { get; set; }
        public string? ObjectType { get; set; }
        public string? SystemStatus { get; set; }
        public string? UserStatus { get; set; }
        public decimal? AcquisitionValue { get; set; }
        public string? Comments { get; set; }

        // Tracking fields
        public string? TagIdentifier { get; set; }
        public DateTime? LastDiscoveredAt { get; set; }
        public string? LastDiscoveredBy { get; set; }
        public int? CurrentLocationId { get; set; }
        public string? CurrentLocationName { get; set; }

        // System fields
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
    }
}



