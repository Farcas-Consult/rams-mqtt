using System.Collections.Generic;
using ZebraIoTConnector.DomainModel.Dto;

namespace ZebraIoTConnector.Services
{
    public interface IAssetManagementService
    {
        AssetDto CreateAsset(CreateAssetDto dto);
        AssetDto UpdateAsset(int id, UpdateAssetDto dto);
        AssetDto? GetAsset(int id);
        AssetDto? GetAssetByTag(string tagIdentifier);
        (List<AssetDto> Assets, int TotalCount) GetAssets(AssetFilterDto filter);
        void DeleteAsset(int id);
        void AssignTagToAsset(int assetId, string tagIdentifier);
        void UnassignTag(int assetId);
        AssetImportResultDto BulkImportAssets(List<BulkImportAssetDto> assets);
    }
}

