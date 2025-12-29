using Microsoft.AspNetCore.Mvc;
using ZebraIoTConnector.DomainModel.Dto;
using ZebraIoTConnector.Services;

namespace ZebraIoTConnector.Backend.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    public class GatesController : ControllerBase
    {
        private readonly IGateManagementService gateManagementService;
        private readonly ILogger<GatesController> logger;

        public GatesController(IGateManagementService gateManagementService, ILogger<GatesController> logger)
        {
            this.gateManagementService = gateManagementService ?? throw new ArgumentNullException(nameof(gateManagementService));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all gates
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<GateDto>), StatusCodes.Status200OK)]
        public ActionResult<List<GateDto>> GetAllGates()
        {
            try
            {
                var gates = gateManagementService.GetAllGates();
                return Ok(gates);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving gates");
                return StatusCode(500, "An error occurred while retrieving gates");
            }
        }

        /// <summary>
        /// Get gate by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(GateDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<GateDto> GetGate(int id)
        {
            try
            {
                var gate = gateManagementService.GetGate(id);
                if (gate == null)
                    return NotFound();

                return Ok(gate);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error retrieving gate {id}");
                return StatusCode(500, "An error occurred while retrieving the gate");
            }
        }

        /// <summary>
        /// Create a new gate
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(GateDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<GateDto> CreateGate([FromBody] CreateGateDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var gate = gateManagementService.CreateGate(dto);
                return CreatedAtAction(nameof(GetGate), new { id = gate.Id }, gate);
            }
            catch (ArgumentException ex)
            {
                logger.LogWarning(ex, "Invalid request to create gate");
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                logger.LogWarning(ex, "Invalid operation while creating gate");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating gate: {Message}", ex.Message);
                return StatusCode(500, $"An error occurred while creating the gate: {ex.Message}");
            }
        }

        /// <summary>
        /// Update an existing gate
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(GateDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<GateDto> UpdateGate(int id, [FromBody] UpdateGateDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var gate = gateManagementService.UpdateGate(id, dto);
                return Ok(gate);
            }
            catch (ArgumentException ex)
            {
                logger.LogWarning(ex, $"Invalid request to update gate {id}");
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error updating gate {id}");
                return StatusCode(500, "An error occurred while updating the gate");
            }
        }

        /// <summary>
        /// Assign a reader to a gate
        /// </summary>
        [HttpPost("{gateId}/readers/{readerId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult AssignReaderToGate(int gateId, int readerId)
        {
            try
            {
                gateManagementService.AssignReaderToGate(gateId, readerId);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                logger.LogWarning(ex, $"Invalid request to assign reader {readerId} to gate {gateId}");
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error assigning reader {readerId} to gate {gateId}");
                return StatusCode(500, "An error occurred while assigning the reader to the gate");
            }
        }

        /// <summary>
        /// Remove a reader from a gate
        /// </summary>
        [HttpDelete("{gateId}/readers/{readerId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult RemoveReaderFromGate(int gateId, int readerId)
        {
            try
            {
                gateManagementService.RemoveReaderFromGate(gateId, readerId);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error removing reader {readerId} from gate {gateId}");
                return StatusCode(500, "An error occurred while removing the reader from the gate");
            }
        }
    }
}

