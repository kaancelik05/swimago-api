using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swimago.Application.DTOs.Admin.Listings;
using Swimago.Application.DTOs.Admin.Shared;
using Swimago.Application.Interfaces;
using Swimago.Domain.Enums;

namespace Swimago.API.Controllers.Admin;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/admin")]
[Produces("application/json")]
public class AdminListingsController : ControllerBase
{
    private readonly IAdminListingService _listingService;

    public AdminListingsController(IAdminListingService listingService)
    {
        _listingService = listingService;
    }

    // --- BEACHES ---

    [HttpGet("beaches")]
    [ProducesResponseType(typeof(PaginatedResponse<ListingListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBeaches(
        [FromQuery] string? search,
        [FromQuery] string? city,
        [FromQuery] bool? isActive,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _listingService.GetListingsAsync(ListingType.Beach, search, city, isActive, minPrice, maxPrice, page, pageSize, cancellationToken);
        return Ok(result);
    }

    [HttpGet("beaches/{id}")]
    [ProducesResponseType(typeof(BeachDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBeach(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _listingService.GetBeachAsync(id, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "Beach not found" });
        }
    }

    [HttpPost("beaches")]
    [ProducesResponseType(typeof(BeachDetailDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateBeach([FromBody] CreateBeachRequest request, CancellationToken cancellationToken)
    {
        var result = await _listingService.CreateBeachAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetBeach), new { id = result.Id }, result);
    }

    [HttpPut("beaches/{id}")]
    [ProducesResponseType(typeof(BeachDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateBeach(Guid id, [FromBody] CreateBeachRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _listingService.UpdateBeachAsync(id, request, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "Beach not found" });
        }
    }

    [HttpDelete("beaches/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteBeach(Guid id, CancellationToken cancellationToken)
    {
        await _listingService.DeleteListingAsync(id, cancellationToken);
        return NoContent();
    }

    // --- OTHER TYPES (Implement similarly as needed or separate controllers) ---
    // For brevity in this turn, focusing on Beach as the primary example.
    // Pools, Yacht Tours, Day Trips would follow identical patterns using their specific DTOs.
}
