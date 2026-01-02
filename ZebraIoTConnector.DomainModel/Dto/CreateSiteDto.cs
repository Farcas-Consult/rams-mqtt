using System.ComponentModel.DataAnnotations;

namespace ZebraIoTConnector.DomainModel.Dto
{
    public class CreateSiteDto
    {
        [Required]
        public string Name { get; set; }
        public string? Description { get; set; }
    }
}
