using Microsoft.AspNetCore.Mvc;
using ZebraIoTConnector.DomainModel.Dto;
using ZebraIoTConnector.Services;

namespace ZebraIoTConnector.Backend.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    public class AssetsController : ControllerBase
    {
        private readonly IAssetManagementService assetManagementService;
        private readonly ILogger<AssetsController> logger;

        public AssetsController(IAssetManagementService assetManagementService, ILogger<AssetsController> logger)
        {
            this.assetManagementService = assetManagementService ?? throw new ArgumentNullException(nameof(assetManagementService));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all assets with optional filters
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<AssetDto>), StatusCodes.Status200OK)]
        public ActionResult<List<AssetDto>> GetAssets([FromQuery] AssetFilterDto filter)
        {
            try
            {
                var (assets, totalCount) = assetManagementService.GetAssets(filter);
                Response.Headers.Add("X-Total-Count", totalCount.ToString());
                return Ok(assets);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving assets");
                return StatusCode(500, "An error occurred while retrieving assets");
            }
        }

        /// <summary>
        /// Get asset by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(AssetDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<AssetDto> GetAsset(int id)
        {
            try
            {
                var asset = assetManagementService.GetAsset(id);
                if (asset == null)
                    return NotFound();

                return Ok(asset);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error retrieving asset {id}");
                return StatusCode(500, "An error occurred while retrieving the asset");
            }
        }

        /// <summary>
        /// Get asset by tag identifier
        /// </summary>
        [HttpGet("by-tag/{tagId}")]
        [ProducesResponseType(typeof(AssetDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<AssetDto> GetAssetByTag(string tagId)
        {
            try
            {
                var asset = assetManagementService.GetAssetByTag(tagId);
                if (asset == null)
                    return NotFound();

                return Ok(asset);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error retrieving asset by tag {tagId}");
                return StatusCode(500, "An error occurred while retrieving the asset");
            }
        }

        /// <summary>
        /// Create a new asset
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(AssetDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<AssetDto> CreateAsset([FromBody] CreateAssetDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var asset = assetManagementService.CreateAsset(dto);
                return CreatedAtAction(nameof(GetAsset), new { id = asset.Id }, asset);
            }
            catch (ArgumentException ex)
            {
                logger.LogWarning(ex, "Invalid request to create asset");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating asset");
                return StatusCode(500, "An error occurred while creating the asset");
            }
        }

        /// <summary>
        /// Update an existing asset
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(AssetDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<AssetDto> UpdateAsset(int id, [FromBody] UpdateAssetDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var asset = assetManagementService.UpdateAsset(id, dto);
                return Ok(asset);
            }
            catch (ArgumentException ex)
            {
                logger.LogWarning(ex, $"Invalid request to update asset {id}");
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error updating asset {id}");
                return StatusCode(500, "An error occurred while updating the asset");
            }
        }

        /// <summary>
        /// Delete an asset (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult DeleteAsset(int id)
        {
            try
            {
                assetManagementService.DeleteAsset(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error deleting asset {id}");
                return StatusCode(500, "An error occurred while deleting the asset");
            }
        }

        /// <summary>
        /// Assign a tag to an asset
        /// </summary>
        [HttpPost("{id}/assign-tag")]
        [ProducesResponseType(typeof(AssetDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<AssetDto> AssignTag(int id, [FromBody] AssignTagRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.TagIdentifier))
                    return BadRequest("Tag identifier is required");

                assetManagementService.AssignTagToAsset(id, request.TagIdentifier);
                var asset = assetManagementService.GetAsset(id);
                return Ok(asset);
            }
            catch (ArgumentException ex)
            {
                logger.LogWarning(ex, $"Invalid request to assign tag to asset {id}");
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error assigning tag to asset {id}");
                return StatusCode(500, "An error occurred while assigning the tag");
            }
        }

        /// <summary>
        /// Unassign tag from an asset
        /// </summary>
        [HttpPost("{id}/unassign-tag")]
        [ProducesResponseType(typeof(AssetDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<AssetDto> UnassignTag(int id)
        {
            try
            {
                assetManagementService.UnassignTag(id);
                var asset = assetManagementService.GetAsset(id);
                return Ok(asset);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error unassigning tag from asset {id}");
                return StatusCode(500, "An error occurred while unassigning the tag");
            }
        }

        /// <summary>
        /// Bulk import assets
        /// </summary>
        [HttpPost("bulk-import")]
        [ProducesResponseType(typeof(AssetImportResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<AssetImportResultDto> BulkImport([FromBody] List<BulkImportAssetDto> assets)
        {
            try
            {
                if (assets == null || assets.Count == 0)
                    return BadRequest("Assets list cannot be empty");

                var result = assetManagementService.BulkImportAssets(assets);
                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during bulk import");
                return StatusCode(500, "An error occurred during bulk import");
            }
        }
    }

    public class AssignTagRequest
    {
        public string TagIdentifier { get; set; } = string.Empty;
    }
}

