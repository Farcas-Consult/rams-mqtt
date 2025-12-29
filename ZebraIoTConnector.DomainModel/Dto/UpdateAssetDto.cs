namespace ZebraIoTConnector.DomainModel.Dto
{
    public class UpdateAssetDto
    {
        public string? Name { get; set; }
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
        public int? CurrentLocationId { get; set; }
    }
}



