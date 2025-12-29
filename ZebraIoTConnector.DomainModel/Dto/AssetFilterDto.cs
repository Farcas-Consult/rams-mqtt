using System;

namespace ZebraIoTConnector.DomainModel.Dto
{
    public class AssetFilterDto
    {
        public string? Plant { get; set; }
        public string? CostCenter { get; set; }
        public int? LocationId { get; set; }
        public string? AssetNumber { get; set; }
        public string? Name { get; set; }
        public string? TagIdentifier { get; set; }
        public int? DaysNotSeen { get; set; }
        public bool? IsDeleted { get; set; }
        
        // Pagination
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }
}

