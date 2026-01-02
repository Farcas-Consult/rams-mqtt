
using System.Collections.Generic;

namespace ZebraIoTConnector.DomainModel.Dto
{
    public class SiteDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public List<GateDto> Gates { get; set; } = new List<GateDto>();
    }
}
