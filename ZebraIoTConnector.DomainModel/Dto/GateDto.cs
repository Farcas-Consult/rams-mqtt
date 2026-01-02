using System.Collections.Generic;

namespace ZebraIoTConnector.DomainModel.Dto
{
    public class GateDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public int? LocationId { get; set; }
        public string LocationName { get; set; }
        public int? SiteId { get; set; }
        public string SiteName { get; set; }
        public bool IsActive { get; set; }
        public List<EquipmentDto> Readers { get; set; } = new List<EquipmentDto>();
    }

    public class ReaderDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public bool IsMobile { get; set; }
        public bool IsOnline { get; set; }
    }
}

