using Microsoft.AspNetCore.Mvc;
using ZebraIoTConnector.DomainModel.Dto;
using ZebraIoTConnector.Services;

namespace ZebraIoTConnector.Backend.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportingService reportingService;
        private readonly ILogger<ReportsController> logger;

        public ReportsController(IReportingService reportingService, ILogger<ReportsController> logger)
        {
            this.reportingService = reportingService ?? throw new ArgumentNullException(nameof(reportingService));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get location report
        /// </summary>
        [HttpGet("location")]
        [ProducesResponseType(typeof(List<AssetDto>), StatusCodes.Status200OK)]
        public ActionResult<List<AssetDto>> GetLocationReport([FromQuery] LocationReportFilterDto filter)
        {
            try
            {
                var assets = reportingService.GetLocationReport(filter);
                return Ok(assets);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving location report");
                return StatusCode(500, "An error occurred while retrieving the location report");
            }
        }

        /// <summary>
        /// Get movement report
        /// </summary>
        [HttpGet("movement")]
        [ProducesResponseType(typeof(List<AssetMovementDto>), StatusCodes.Status200OK)]
        public ActionResult<List<AssetMovementDto>> GetMovementReport([FromQuery] MovementReportFilterDto filter)
        {
            try
            {
                var movements = reportingService.GetMovementReport(filter);
                return Ok(movements);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving movement report");
                return StatusCode(500, "An error occurred while retrieving the movement report");
            }
        }

        /// <summary>
        /// Get discovery report (assets not seen for specified days)
        /// </summary>
        [HttpGet("discovery")]
        [ProducesResponseType(typeof(List<AssetDto>), StatusCodes.Status200OK)]
        public ActionResult<List<AssetDto>> GetDiscoveryReport([FromQuery] int daysNotSeen = 30)
        {
            try
            {
                var assets = reportingService.GetDiscoveryReport(daysNotSeen);
                return Ok(assets);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error retrieving discovery report for {daysNotSeen} days");
                return StatusCode(500, "An error occurred while retrieving the discovery report");
            }
        }

        /// <summary>
        /// Get gate activity report
        /// </summary>
        [HttpGet("gate-activity")]
        [ProducesResponseType(typeof(List<AssetMovementDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<List<AssetMovementDto>> GetGateActivityReport(
            [FromQuery] int gateId,
            [FromQuery] DateTime from,
            [FromQuery] DateTime to)
        {
            try
            {
                if (from > to)
                    return BadRequest("From date must be before To date");

                var movements = reportingService.GetGateActivityReport(gateId, from, to);
                return Ok(movements);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error retrieving gate activity report for gate {gateId}");
                return StatusCode(500, "An error occurred while retrieving the gate activity report");
            }
        }

        /// <summary>
        /// Get asset statistics
        /// </summary>
        [HttpGet("statistics")]
        [ProducesResponseType(typeof(AssetStatisticsDto), StatusCodes.Status200OK)]
        public ActionResult<AssetStatisticsDto> GetStatistics()
        {
            try
            {
                var statistics = reportingService.GetAssetStatistics();
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving statistics");
                return StatusCode(500, "An error occurred while retrieving statistics");
            }
        }
    }
}

