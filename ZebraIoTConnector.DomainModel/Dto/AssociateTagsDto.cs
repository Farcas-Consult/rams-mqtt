using System.Collections.Generic;

namespace ZebraIoTConnector.DomainModel.Dto
{
    public class AssociateTagsDto
    {
        public string AssetNumber { get; set; } // The Barcode/Equipment ID
        public List<AssetTagDto> Tags { get; set; } = new List<AssetTagDto>();
    }
}
