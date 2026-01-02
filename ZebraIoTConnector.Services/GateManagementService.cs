using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using ZebraIoTConnector.DomainModel.Dto;
using ZebraIoTConnector.Persistence;
using ZebraIoTConnector.Persistence.Entities;

namespace ZebraIoTConnector.Services
{
    public class GateManagementService : IGateManagementService
    {
        private readonly ILogger<GateManagementService> logger;
        private readonly IUnitOfWork unitOfWork;

        public GateManagementService(ILogger<GateManagementService> logger, IUnitOfWork unitOfWork)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public GateDto CreateGate(CreateGateDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Name is required", nameof(dto));

            // Check for duplicate name
            var existingGate = unitOfWork.GateRepository.GetByName(dto.Name);
            if (existingGate != null)
                throw new InvalidOperationException($"Gate with name '{dto.Name}' already exists");

            var gate = new Gate
            {
                Name = dto.Name,
                Description = dto.Description,
                LocationId = dto.LocationId,
                SiteId = dto.SiteId,
                IsActive = dto.IsActive
            };

            // Auto-assign LocationId from Site if missing
            if (!gate.LocationId.HasValue && gate.SiteId.HasValue)
            {
                var site = unitOfWork.SiteRepository.GetById(gate.SiteId.Value);
                if (site != null)
                {
                    var storageUnit = unitOfWork.StorageUnitRepository.GetByName(site.Name);
                    if (storageUnit != null)
                    {
                        gate.LocationId = storageUnit.Id;
                    }
                }
            }

            unitOfWork.GateRepository.Create(gate);

            // Reload the gate with navigation properties to properly map to DTO
            var createdGate = unitOfWork.GateRepository.GetById(gate.Id);
            if (createdGate == null)
                throw new InvalidOperationException("Failed to retrieve created gate");

            return MapToDto(createdGate);
        }

        public GateDto UpdateGate(int id, UpdateGateDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var gate = unitOfWork.GateRepository.GetById(id);
            if (gate == null)
                throw new InvalidOperationException($"Gate with ID {id} not found");

            // Check for duplicate name if being changed
            if (!string.IsNullOrWhiteSpace(dto.Name) && dto.Name != gate.Name)
            {
                var existingGate = unitOfWork.GateRepository.GetByName(dto.Name);
                if (existingGate != null)
                    throw new InvalidOperationException($"Gate with name '{dto.Name}' already exists");
            }

            // Update fields
            if (!string.IsNullOrWhiteSpace(dto.Name))
                gate.Name = dto.Name;

            gate.Description = dto.Description ?? gate.Description;
            
            if (dto.LocationId.HasValue)
                gate.LocationId = dto.LocationId;

            if (dto.SiteId.HasValue)
                gate.SiteId = dto.SiteId;

            if (dto.IsActive.HasValue)
                gate.IsActive = dto.IsActive.Value;

            unitOfWork.GateRepository.Update(gate);

            return MapToDto(gate);
        }

        public GateDto? GetGate(int id)
        {
            var gate = unitOfWork.GateRepository.GetById(id);
            return gate != null ? MapToDto(gate) : null;
        }

        public List<GateDto> GetAllGates()
        {
            var gates = unitOfWork.GateRepository.GetAll();
            return gates.Select(MapToDto).ToList();
        }

        public void AssignReaderToGate(int gateId, int readerId)
        {
            var gate = unitOfWork.GateRepository.GetById(gateId);
            if (gate == null)
                throw new InvalidOperationException($"Gate with ID {gateId} not found");

            var reader = unitOfWork.EquipmentRepository.GetEquipmentById(readerId);
            if (reader == null)
                throw new InvalidOperationException($"Reader with ID {readerId} not found");

            // Check if reader is already assigned to a different gate
            if (reader.GateId.HasValue && reader.GateId.Value != gateId)
            {
                var currentGate = unitOfWork.GateRepository.GetById(reader.GateId.Value);
                throw new InvalidOperationException($"Reader is already assigned to gate '{currentGate?.Name}'");
            }

            // Since GetEquipmentById uses the same DbContext as UnitOfWork, the entity is tracked.
            // Modify the entity and save changes via UnitOfWork.
            reader.GateId = gateId;
            unitOfWork.SaveChanges();
        }

        public void RemoveReaderFromGate(int gateId, int readerId)
        {
            var gate = unitOfWork.GateRepository.GetById(gateId);
            if (gate == null)
                throw new InvalidOperationException($"Gate with ID {gateId} not found");

            var reader = unitOfWork.EquipmentRepository.GetEquipmentById(readerId);
            if (reader == null)
                throw new InvalidOperationException($"Reader with ID {readerId} not found");

            if (reader.GateId != gateId)
                throw new InvalidOperationException($"Reader is not assigned to gate '{gate.Name}'");

            reader.GateId = null;
            unitOfWork.SaveChanges();
        }

        public GateDto? GetGateByReaderName(string readerName)
        {
            if (string.IsNullOrWhiteSpace(readerName))
                return null;

            var gate = unitOfWork.GateRepository.GetByReaderName(readerName);
            return gate != null ? MapToDto(gate) : null;
        }

        private GateDto MapToDto(Gate gate)
        {
            if (gate == null)
                throw new ArgumentNullException(nameof(gate));

            return new GateDto
            {
                Id = gate.Id,
                Name = gate.Name,
                Description = gate.Description,
                LocationId = gate.LocationId,
                LocationName = gate.Location?.Name,
                SiteId = gate.SiteId,
                SiteName = gate.Site?.Name,
                IsActive = gate.IsActive,
                Readers = gate.Readers?.Select(r => new EquipmentDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description,
                    IsMobile = r.IsMobile,
                    IsOnline = r.IsOnline
                }).ToList() ?? new List<EquipmentDto>()
            };
        }
    }
}

