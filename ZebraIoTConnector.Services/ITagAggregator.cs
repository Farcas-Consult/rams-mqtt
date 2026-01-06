using System;

namespace ZebraIoTConnector.Services
{
    public class TagReadContext
    {
        public string AssetId { get; set; } // DB ID (int parsed to string or just string)
        public string AssetType { get; set; } // "Container", "Vehicle", "Normal"
        public string EPC { get; set; }
        public int AntennaId { get; set; }
        public DateTime Timestamp { get; set; }
        public string ReaderId { get; set; }
        public int GateId { get; set; }
        public string RequiredTags { get; set; } // Optional: Comma separated list of required tags if known
    }

    public interface ITagAggregator
    {
        void AddTagRead(TagReadContext context);
    }
}
