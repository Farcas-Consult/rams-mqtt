namespace ZebraIoTConnector.DomainModel.Dto
{
    public class AssetStatisticsDto
    {
        public int TotalAssets { get; set; }
        public int AssetsWithTags { get; set; }
        public int AssetsWithoutTags { get; set; }
        public int ActiveAssets { get; set; }
        public int AssetsNotSeenIn30Days { get; set; }
        public int AssetsNotSeenIn90Days { get; set; }
        public int TotalGates { get; set; }
        public int ActiveGates { get; set; }
        public int TotalMovements { get; set; }
    }
}

