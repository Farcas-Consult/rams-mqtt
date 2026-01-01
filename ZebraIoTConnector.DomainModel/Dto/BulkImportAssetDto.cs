namespace ZebraIoTConnector.DomainModel.Dto
{
    public class BulkImportAssetDto
    {
        public string Equipment { get; set; } // Maps to AssetNumber
        public string Description { get; set; } // Maps to Name
        public string? MaterialDescription { get; set; } // Maps to Description
        public string? Material { get; set; } // Maps to MaterialId
        public string? ManufSerialNumber { get; set; } // Maps to SerialNumber
        public string? TechIdentNo { get; set; } // Maps to TechnicalId
        public string? Plnt { get; set; } // Maps to Plant
        public string? SLoc { get; set; } // Maps to StorageLocation
        public string? CostCtr { get; set; } // Maps to CostCenter
        public string? AGrp { get; set; } // Maps to AssetGroup
        public string? BusA { get; set; } // Maps to BusinessArea
        public string? ObjectType { get; set; }
        public string? SysStatus { get; set; } // Maps to SystemStatus
        public string? UserStatus { get; set; }
        public decimal? AcquistnValue { get; set; } // Maps to AcquisitionValue
        public string? Comment { get; set; } // Maps to Comments

        // Expanded fields
        public string? Manufacturer { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public string? Category { get; set; }
        public string? Location { get; set; }
    }
}



