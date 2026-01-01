using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZebraIoTConnector.Persistence.Entities
{
    [Index(nameof(AssetNumber), IsUnique = true)]
    [Index(nameof(TagIdentifier), IsUnique = true)]
    [Index(nameof(LastDiscoveredAt))]
    [Index(nameof(Plant))]
    [Index(nameof(CostCenter))]
    public class Asset
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // Core fields
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

        // Expanded Import fields
        public string? Manufacturer { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public string? Category { get; set; }
        public string? Location { get; set; } // Friendly location name

        // Tracking fields
        public string? TagIdentifier { get; set; }
        public DateTime? LastDiscoveredAt { get; set; }
        public string? LastDiscoveredBy { get; set; }
        public int? CurrentLocationId { get; set; }

        // System fields
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }

        // Navigation properties
        public StorageUnit? CurrentLocation { get; set; }
        public List<AssetMovement> Movements { get; set; } = new List<AssetMovement>();
    }
}



