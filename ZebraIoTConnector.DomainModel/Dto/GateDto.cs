using System.Collections.Generic;

namespace ZebraIoTConnector.DomainModel.Dto
{
    public class GateDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public int? LocationId { get; set; }
        public string? LocationName { get; set; }
        public bool IsActive { get; set; }
        public List<ReaderDto> Readers { get; set; } = new List<ReaderDto>();
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

