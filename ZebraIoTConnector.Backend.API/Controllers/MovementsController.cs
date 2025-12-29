using Microsoft.AspNetCore.Mvc;
using ZebraIoTConnector.DomainModel.Dto;
using ZebraIoTConnector.Persistence;
using ZebraIoTConnector.Services;

namespace ZebraIoTConnector.Backend.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    public class MovementsController : ControllerBase
    {
        private readonly IReportingService reportingService;
        private readonly IUnitOfWork unitOfWork;
        private readonly ILogger<MovementsController> logger;

        public MovementsController(
            IReportingService reportingService,
            IUnitOfWork unitOfWork,
            ILogger<MovementsController> logger)
        {
            this.reportingService = reportingService ?? throw new ArgumentNullException(nameof(reportingService));
            this.unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get movement history with optional filters
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<AssetMovementDto>), StatusCodes.Status200OK)]
        public ActionResult<List<AssetMovementDto>> GetMovements([FromQuery] MovementReportFilterDto filter)
        {
            try
            {
                var movements = reportingService.GetMovementReport(filter);
                return Ok(movements);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving movements");
                return StatusCode(500, "An error occurred while retrieving movements");
            }
        }

        /// <summary>
        /// Get movement by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(AssetMovementDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<AssetMovementDto> GetMovement(int id)
        {
            try
            {
                var movement = unitOfWork.AssetMovementRepository.GetById(id);
                if (movement == null)
                    return NotFound();

                var dto = MapToDto(movement);
                return Ok(dto);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error retrieving movement {id}");
                return StatusCode(500, "An error occurred while retrieving the movement");
            }
        }

        private AssetMovementDto MapToDto(ZebraIoTConnector.Persistence.Entities.AssetMovement movement)
        {
            return new AssetMovementDto
            {
                Id = movement.Id,
                AssetId = movement.AssetId,
                AssetNumber = movement.Asset?.AssetNumber,
                AssetName = movement.Asset?.Name,
                FromLocationId = movement.FromLocationId,
                FromLocationName = movement.FromLocation?.Name,
                ToLocationId = movement.ToLocationId,
                ToLocationName = movement.ToLocation?.Name,
                GateId = movement.GateId,
                GateName = movement.Gate?.Name,
                ReaderId = movement.ReaderId,
                ReaderName = movement.Reader?.Name,
                ReaderIdString = movement.ReaderIdString,
                ReadTimestamp = movement.ReadTimestamp
            };
        }
    }
}

