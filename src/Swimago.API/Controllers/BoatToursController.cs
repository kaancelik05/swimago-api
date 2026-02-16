using Microsoft.AspNetCore.Mvc;
using Swimago.Application.DTOs.BoatTours;
using Swimago.Application.Interfaces;

namespace Swimago.API.Controllers;

[ApiController]
[Route("api/boat-tours")]
[Produces("application/json")]
public class BoatToursController : ControllerBase
{
    private readonly IBoatTourService _boatTourService;
    private readonly ILogger<BoatToursController> _logger;

    public BoatToursController(
        IBoatTourService boatTourService,
        ILogger<BoatToursController> logger)
    {
        _boatTourService = boatTourService;
        _logger = logger;
    }

    /// <summary>
    /// Get all boat tours (yachts and day trips)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(BoatTourListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? city,
        [FromQuery] string? type,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        CancellationToken cancellationToken)
    {
        var result = await _boatTourService.GetAllBoatToursAsync(city, type, minPrice, maxPrice, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get yacht tour details by slug
    /// </summary>
    [HttpGet("yacht/{slug}")]
    [ProducesResponseType(typeof(YachtTourDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetYachtBySlug(string slug, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _boatTourService.GetYachtTourBySlugAsync(slug, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get day trip details by slug
    /// </summary>
    [HttpGet("day-trip/{slug}")]
    [ProducesResponseType(typeof(DayTripDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDayTripBySlug(string slug, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _boatTourService.GetDayTripBySlugAsync(slug, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}
