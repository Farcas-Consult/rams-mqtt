using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZebraIoTConnector.Persistence.Entities
{
    [Index(nameof(Name), IsUnique = true)]
    public class Site
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Name { get; set; }
        public string? Description { get; set; }

        // Navigation property
        public List<Gate> Gates { get; set; } = new List<Gate>();
    }
}
