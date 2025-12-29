using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZebraIoTConnector.Persistence.Entities
{
    [Index(nameof(Name), IsUnique = true)]
    public class Gate
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Name { get; set; }
        public string? Description { get; set; }
        public int? LocationId { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public StorageUnit? Location { get; set; }
        public List<Equipment> Readers { get; set; } = new List<Equipment>();
    }
}



