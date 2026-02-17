using Microsoft.AspNetCore.Mvc;
using Swimago.Application.DTOs.Destinations;
using Swimago.Application.Interfaces;

namespace Swimago.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DestinationsController : ControllerBase
{
    private readonly IDestinationService _destinationService;
    private readonly ILogger<DestinationsController> _logger;

    public DestinationsController(
        IDestinationService destinationService,
        ILogger<DestinationsController> logger)
    {
        _destinationService = destinationService;
        _logger = logger;
    }

    /// <summary>
    /// Get all destinations (cities with venues)
    /// </summary>
    /// <param name="featured">Filter by featured status</param>
    /// <param name="type">Filter by destination type (Beach|Pool)</param>
    /// <param name="search">Search text for city/country</param>
    /// <param name="page">Page number (starts at 1)</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of destinations</returns>
    /// <response code="200">Returns destinations list</response>
    [HttpGet]
    [ProducesResponseType(typeof(DestinationListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool? featured,
        [FromQuery] string? type,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _destinationService.GetAllDestinationsAsync(
            featured,
            type,
            search,
            page,
            pageSize,
            cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get destination details by slug
    /// </summary>
    /// <param name="slug">Destination slug</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Destination details with spots</returns>
    /// <response code="200">Returns destination details</response>
    /// <response code="404">Destination not found</response>
    [HttpGet("{slug}")]
    [ProducesResponseType(typeof(DestinationDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _destinationService.GetDestinationBySlugAsync(slug, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get customer destination detail page model by slug
    /// </summary>
    /// <param name="slug">Destination slug</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Destination detail page payload</returns>
    [HttpGet("{slug}/detail")]
    [ProducesResponseType(typeof(DestinationPageDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPageDetailBySlug(string slug, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _destinationService.GetDestinationPageBySlugAsync(slug, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}
