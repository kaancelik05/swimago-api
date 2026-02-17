using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swimago.API.Authorization;
using Swimago.Application.DTOs.Admin.Destinations;
using Swimago.Application.DTOs.Admin.Shared;
using Swimago.Application.Interfaces;

namespace Swimago.API.Controllers.Admin;

[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[ApiController]
[Route("api/admin/destinations")]
[Produces("application/json")]
public class AdminDestinationsController : ControllerBase
{
    private readonly IAdminDestinationService _destinationService;

    public AdminDestinationsController(IAdminDestinationService destinationService)
    {
        _destinationService = destinationService;
    }

    /// <summary>
    /// List destinations with pagination and filtering
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<DestinationListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDestinations(
        [FromQuery] string? search,
        [FromQuery] string? country,
        [FromQuery] bool? isFeatured,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _destinationService.GetDestinationsAsync(search, country, isFeatured, page, pageSize, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get destination details
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(DestinationDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDestination(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _destinationService.GetDestinationAsync(id, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "Destination not found" });
        }
    }

    /// <summary>
    /// Create new destination
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(DestinationDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateDestination([FromBody] CreateDestinationRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _destinationService.CreateDestinationAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetDestination), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update existing destination
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(DestinationDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateDestination(Guid id, [FromBody] CreateDestinationRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _destinationService.UpdateDestinationAsync(id, request, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "Destination not found" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Delete destination
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteDestination(Guid id, CancellationToken cancellationToken)
    {
        await _destinationService.DeleteDestinationAsync(id, cancellationToken);
        return NoContent();
    }
}
