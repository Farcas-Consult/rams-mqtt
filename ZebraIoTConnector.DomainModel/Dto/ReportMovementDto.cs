using System;

namespace ZebraIoTConnector.DomainModel.Dto
{
    public class ReportMovementDto
    {
        public string TagId { get; set; }
        public int GateId { get; set; }
        public string DeviceId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
