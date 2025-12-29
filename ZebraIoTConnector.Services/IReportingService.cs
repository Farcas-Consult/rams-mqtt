using System;
using System.Collections.Generic;
using ZebraIoTConnector.DomainModel.Dto;

namespace ZebraIoTConnector.Services
{
    public interface IReportingService
    {
        List<AssetDto> GetLocationReport(LocationReportFilterDto filter);
        List<AssetMovementDto> GetMovementReport(MovementReportFilterDto filter);
        List<AssetDto> GetDiscoveryReport(int daysNotSeen);
        List<AssetMovementDto> GetGateActivityReport(int gateId, DateTime from, DateTime to);
        AssetStatisticsDto GetAssetStatistics();
    }
}

