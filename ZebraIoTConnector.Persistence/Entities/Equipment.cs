using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZebraIoTConnector.Persistence.Entities
{
    [Index(nameof(Name), IsUnique = true)]
    public class Equipment
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public StorageUnit? ReferenceStorageUnit { get; set; }

        // Asset tracking fields
        public int? GateId { get; set; }
        public bool IsMobile { get; set; } = false;
        public DateTime? LastHeartbeat { get; set; }
        public bool IsOnline { get; set; } = false;

        // Navigation properties
        public Gate? Gate { get; set; }
    }
}
