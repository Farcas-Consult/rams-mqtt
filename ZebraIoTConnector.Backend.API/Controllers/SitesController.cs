using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using ZebraIoTConnector.DomainModel.Dto;
using ZebraIoTConnector.Persistence;
using ZebraIoTConnector.Persistence.Entities;
using Microsoft.Extensions.Logging;

namespace ZebraIoTConnector.Backend.API.Controllers
{
    [ApiController]
    [Route("api/v1/sites")]
    public class SitesController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ILogger<SitesController> logger;

        public SitesController(IUnitOfWork unitOfWork, ILogger<SitesController> logger)
        {
            this.unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public ActionResult<IEnumerable<SiteDto>> GetAll()
        {
            try
            {
                var sites = unitOfWork.SiteRepository.GetAll();
                return Ok(sites.Select(MapToDto));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching sites");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public ActionResult<SiteDto> GetById(int id)
        {
            try
            {
                var site = unitOfWork.SiteRepository.GetById(id);
                if (site == null)
                    return NotFound();

                return Ok(MapToDto(site));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching site {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}/gates")]
        public ActionResult<IEnumerable<GateDto>> GetGatesBySite(int id)
        {
            try
            {
                var site = unitOfWork.SiteRepository.GetById(id);
                if (site == null)
                    return NotFound();

                // Assuming Gates are loaded via Include in Repository
                var gates = site.Gates.Select(g => new GateDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    Description = g.Description,
                    LocationId = g.LocationId,
                    LocationName = g.Location?.Name,
                    SiteId = g.SiteId,
                    SiteName = g.Site?.Name,
                    IsActive = g.IsActive,
                    Readers = g.Readers?.Select(r => new EquipmentDto
                    {
                        Id = r.Id,
                        Name = r.Name,
                        Description = r.Description,
                        IsMobile = r.IsMobile,
                        IsOnline = r.IsOnline
                    }).ToList() ?? new List<EquipmentDto>()
                });

                return Ok(gates);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching gates for site {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public ActionResult<SiteDto> Create([FromBody] CreateSiteDto dto)
        {
            if (dto == null)
                return BadRequest();

            try
            {
                var existing = unitOfWork.SiteRepository.GetByName(dto.Name);
                if (existing != null)
                    return Conflict($"Site with name '{dto.Name}' already exists");

                var site = new Site
                {
                    Name = dto.Name,
                    Description = dto.Description
                };

                unitOfWork.SiteRepository.Create(site);

                // Fetch again to ensure ID and relationships
                var created = unitOfWork.SiteRepository.GetById(site.Id);
                return CreatedAtAction(nameof(GetById), new { id = site.Id }, MapToDto(created));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating site");
                return StatusCode(500, "Internal server error");
            }
        }

        private SiteDto MapToDto(Site site)
        {
            return new SiteDto
            {
                Id = site.Id,
                Name = site.Name,
                Description = site.Description,
                Gates = site.Gates?.Select(g => new GateDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    Description = g.Description,
                    LocationId = g.LocationId,
                    LocationName = g.Location?.Name,
                    SiteId = g.SiteId,
                    SiteName = g.Site?.Name,
                    IsActive = g.IsActive
                }).ToList() ?? new List<GateDto>()
            };
        }
    }
}
