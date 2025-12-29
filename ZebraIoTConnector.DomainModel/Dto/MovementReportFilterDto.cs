using System;

namespace ZebraIoTConnector.DomainModel.Dto
{
    public class MovementReportFilterDto
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public int? AssetId { get; set; }
        public int? GateId { get; set; }
        public int? LocationId { get; set; }
    }
}

