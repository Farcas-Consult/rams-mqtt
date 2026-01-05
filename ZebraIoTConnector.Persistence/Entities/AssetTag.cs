using System.ComponentModel.DataAnnotations.Schema;

namespace ZebraIoTConnector.Persistence.Entities
{
    public class AssetTag
    {
        public int Id { get; set; }
        
        public string TagId { get; set; } // EPC
        public string Location { get; set; } // e.g., "Door", "NearLeft"

        public int AssetId { get; set; }
        public Asset Asset { get; set; }
    }
}
