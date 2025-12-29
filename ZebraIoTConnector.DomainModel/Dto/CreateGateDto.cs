namespace ZebraIoTConnector.DomainModel.Dto
{
    public class CreateGateDto
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public int? LocationId { get; set; }
        public bool IsActive { get; set; } = true;
    }
}

