using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swimago.API.Authorization;
using Swimago.Application.DTOs.Favorites;
using Swimago.Application.Interfaces;
using Swimago.Domain.Enums;
using System.Security.Claims;

namespace Swimago.API.Controllers;

[Authorize(Policy = AuthorizationPolicies.CustomerOnly)]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class FavoritesController : ControllerBase
{
    private readonly IFavoriteService _favoriteService;
    private readonly ILogger<FavoritesController> _logger;

    public FavoritesController(
        IFavoriteService favoriteService,
        ILogger<FavoritesController> logger)
    {
        _favoriteService = favoriteService;
        _logger = logger;
    }

    /// <summary>
    /// Get user's favorites list
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(FavoriteListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] VenueType? type,
        [FromQuery] string? search,
        [FromQuery] string? sortBy,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _favoriteService.GetFavoritesAsync(userId, type, search, sortBy, page, pageSize, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Add venue to favorites
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(FavoriteItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Add([FromBody] AddFavoriteRequest request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        try
        {
            var result = await _favoriteService.AddFavoriteAsync(userId, request, cancellationToken);
            return CreatedAtAction(nameof(GetAll), result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Remove venue from favorites
    /// </summary>
    [HttpDelete("{venueId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remove(Guid venueId, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        try
        {
            await _favoriteService.RemoveFavoriteAsync(userId, venueId, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}
