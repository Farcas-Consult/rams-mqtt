using System.Collections.Generic;

namespace ZebraIoTConnector.DomainModel.Dto
{
    public class AssetImportResultDto
    {
        public bool Success { get; set; }
        public int TotalRows { get; set; }
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public List<AssetImportErrorDto> Errors { get; set; } = new List<AssetImportErrorDto>();
    }

    public class AssetImportErrorDto
    {
        public int RowNumber { get; set; }
        public string? AssetNumber { get; set; }
        public string Error { get; set; }
    }
}



