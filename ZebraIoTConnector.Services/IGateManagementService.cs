using System.Collections.Generic;
using ZebraIoTConnector.DomainModel.Dto;

namespace ZebraIoTConnector.Services
{
    public interface IGateManagementService
    {
        GateDto CreateGate(CreateGateDto dto);
        GateDto UpdateGate(int id, UpdateGateDto dto);
        GateDto? GetGate(int id);
        List<GateDto> GetAllGates();
        void AssignReaderToGate(int gateId, int readerId);
        void RemoveReaderFromGate(int gateId, int readerId);
        GateDto? GetGateByReaderName(string readerName);
    }
}

