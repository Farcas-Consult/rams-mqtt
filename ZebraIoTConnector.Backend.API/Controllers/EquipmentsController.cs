using Microsoft.AspNetCore.Mvc;
using ZebraIoTConnector.DomainModel.Dto;
using ZebraIoTConnector.Persistence;

namespace ZebraIoTConnector.Backend.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    public class EquipmentsController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ILogger<EquipmentsController> logger;

        public EquipmentsController(IUnitOfWork unitOfWork, ILogger<EquipmentsController> logger)
        {
            this.unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all registered equipment (readers)
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<EquipmentDto>), StatusCodes.Status200OK)]
        public ActionResult<List<EquipmentDto>> GetAllEquipments()
        {
            try
            {
                var equipments = unitOfWork.EquipmentRepository.GetEquipments();
                return Ok(equipments);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving equipments");
                return StatusCode(500, "An error occurred while retrieving equipments");
            }
        }

        /// <summary>
        /// Get equipment by name
        /// </summary>
        [HttpGet("by-name/{name}")]
        [ProducesResponseType(typeof(EquipmentDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<EquipmentDto> GetEquipmentByName(string name)
        {
            try
            {
                var equipment = unitOfWork.EquipmentRepository.GetEquipmentByName(name);
                if (equipment == null)
                    return NotFound();

                return Ok(equipment);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error retrieving equipment {name}");
                return StatusCode(500, "An error occurred while retrieving the equipment");
            }
        }
    }
}
