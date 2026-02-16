using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swimago.Application.DTOs.Destinations;
using Swimago.Application.DTOs.Host;
using Swimago.Application.Interfaces;
using Swimago.Domain.Enums;
using System.Security.Claims;

namespace Swimago.API.Controllers;

[Authorize]
[ApiController]
[Route("api/host")]
[Produces("application/json")]
public class HostController : ControllerBase
{
    private readonly IHostService _hostService;
    private readonly ILogger<HostController> _logger;

    public HostController(
        IHostService hostService,
        ILogger<HostController> logger)
    {
        _hostService = hostService;
        _logger = logger;
    }

    /// <summary>
    /// Get host dashboard stats
    /// </summary>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(HostDashboardResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboard(CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _hostService.GetDashboardAsync(userId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get host's own listings
    /// </summary>
    [HttpGet("listings")]
    [ProducesResponseType(typeof(HostListingListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyListings(CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _hostService.GetMyListingsAsync(userId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get host's listing details
    /// </summary>
    [HttpGet("listings/{id}")]
    [ProducesResponseType(typeof(HostListingItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetListing(Guid id, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        try
        {
            var result = await _hostService.GetListingAsync(userId, id, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "İlan bulunamadı" });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    /// <summary>
    /// Update listing basic info
    /// </summary>
    [HttpPut("listings/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateListing(Guid id, [FromBody] UpdateListingRequest request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        try
        {
            await _hostService.UpdateListingAsync(userId, id, request, cancellationToken);
            return NoContent(); // Success, no content needed
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "İlan bulunamadı" });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    /// <summary>
    /// Update listing pricing
    /// </summary>
    [HttpPut("listings/{id}/pricing")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePricing(Guid id, [FromBody] UpdatePricingRequest request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        try
        {
            await _hostService.UpdatePricingAsync(userId, id, request, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "İlan bulunamadı" });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    /// <summary>
    /// Delete (deactivate) listing
    /// </summary>
    [HttpDelete("listings/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteListing(Guid id, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        try
        {
            await _hostService.DeleteListingAsync(userId, id, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "İlan bulunamadı" });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    /// <summary>
    /// Get reservations for host's listings
    /// </summary>
    [HttpGet("reservations")]
    [ProducesResponseType(typeof(HostReservationListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyReservations([FromQuery] ReservationStatus? status, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _hostService.GetReservationsAsync(userId, status, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Update reservation status
    /// </summary>
    [HttpPut("reservations/{id}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateReservationStatus(Guid id, [FromBody] UpdateReservationStatusRequest request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        try
        {
            await _hostService.UpdateReservationStatusAsync(userId, id, request, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "Rezervasyon bulunamadı" });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    /// <summary>
    /// Get listing calendar
    /// </summary>
    [HttpGet("calendar")]
    [ProducesResponseType(typeof(HostCalendarResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCalendar([FromQuery] Guid listingId, [FromQuery] DateTime start, [FromQuery] DateTime end, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        try
        {
            var result = await _hostService.GetCalendarAsync(userId, listingId, start, end, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "İlan bulunamadı" });
        }
    }

    /// <summary>
    /// Update calendar availability/pricing
    /// </summary>
    [HttpPut("calendar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateCalendar([FromBody] UpdateCalendarRequest request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _hostService.UpdateCalendarAsync(userId, request, cancellationToken);
        return Ok(new { message = "Takvim güncellendi" });
    }

    /// <summary>
    /// Get analytics
    /// </summary>
    [HttpGet("analytics")]
    [ProducesResponseType(typeof(HostAnalyticsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAnalytics([FromQuery] DateTime start, [FromQuery] DateTime end, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _hostService.GetAnalyticsAsync(userId, start, end, cancellationToken);
        return Ok(result);
    }
}
