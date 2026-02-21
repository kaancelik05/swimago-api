using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swimago.API.Authorization;
using Swimago.Application.DTOs.Host;
using Swimago.Application.Interfaces;
using System.Security.Claims;

namespace Swimago.API.Controllers;

[Authorize(Policy = AuthorizationPolicies.HostOrAdmin)]
[ApiController]
[Route("api/host")]
[Produces("application/json")]
public class HostController : ControllerBase
{
    private readonly IHostService _hostService;

    public HostController(IHostService hostService)
    {
        _hostService = hostService;
    }

    [HttpGet("listings")]
    [ProducesResponseType(typeof(HostListingsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetListings(
        [FromQuery] string? status,
        [FromQuery] string? type,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _hostService.GetListingsAsync(GetUserId(), status, type, page, pageSize, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("listings/{id:guid}")]
    [ProducesResponseType(typeof(HostListingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetListing(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _hostService.GetListingAsync(GetUserId(), id, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPost("listings")]
    [ProducesResponseType(typeof(HostListingDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateListing([FromBody] UpsertHostListingRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _hostService.CreateListingAsync(GetUserId(), request, cancellationToken);
            return CreatedAtAction(nameof(GetListing), new { id = result.Id }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("listings/{id:guid}")]
    [ProducesResponseType(typeof(HostListingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateListing(Guid id, [FromBody] UpsertHostListingRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _hostService.UpdateListingAsync(GetUserId(), id, request, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPatch("listings/{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateListingStatus(Guid id, [FromBody] UpdateHostListingStatusRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            await _hostService.UpdateListingStatusAsync(GetUserId(), id, request, cancellationToken);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpGet("dashboard/stats")]
    [ProducesResponseType(typeof(DashboardStatsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboardStats(CancellationToken cancellationToken = default)
    {
        var result = await _hostService.GetDashboardStatsAsync(GetUserId(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("reservations/recent")]
    [ProducesResponseType(typeof(IEnumerable<HostReservationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecentReservations([FromQuery] int limit = 7, CancellationToken cancellationToken = default)
    {
        var result = await _hostService.GetRecentReservationsAsync(GetUserId(), limit, cancellationToken);
        return Ok(result);
    }

    [HttpGet("insights")]
    [ProducesResponseType(typeof(IEnumerable<HostInsightDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInsights(CancellationToken cancellationToken = default)
    {
        var result = await _hostService.GetInsightsAsync(GetUserId(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("reservations")]
    [ProducesResponseType(typeof(HostReservationsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetReservations(
        [FromQuery] string? status = "all",
        [FromQuery] string? source = "all",
        [FromQuery] string? listingId = "all",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _hostService.GetReservationsAsync(
                GetUserId(),
                status,
                source,
                listingId,
                page,
                pageSize,
                cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPatch("reservations/{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateReservationStatus(Guid id, [FromBody] UpdateHostReservationStatusRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            await _hostService.UpdateReservationStatusAsync(GetUserId(), id, request, cancellationToken);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPost("reservations/manual")]
    [ProducesResponseType(typeof(HostReservationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateManualReservation([FromBody] CreateManualReservationRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _hostService.CreateManualReservationAsync(GetUserId(), request, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpGet("calendar")]
    [ProducesResponseType(typeof(IEnumerable<CalendarDayDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCalendar(
        [FromQuery] Guid listingId,
        [FromQuery] int month,
        [FromQuery] int year,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _hostService.GetCalendarAsync(GetUserId(), listingId, month, year, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPut("calendar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCalendar([FromBody] UpdateCalendarRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            await _hostService.UpdateCalendarAsync(GetUserId(), request, cancellationToken);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpGet("analytics")]
    [ProducesResponseType(typeof(HostAnalyticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAnalytics(
        [FromQuery] string period = "month",
        [FromQuery] string? listingId = "all",
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _hostService.GetAnalyticsAsync(GetUserId(), period, listingId, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpGet("business-settings")]
    [ProducesResponseType(typeof(BusinessSettingsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBusinessSettings(CancellationToken cancellationToken = default)
    {
        var result = await _hostService.GetBusinessSettingsAsync(GetUserId(), cancellationToken);
        return Ok(result);
    }

    [HttpPut("business-settings")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateBusinessSettings([FromBody] BusinessSettingsDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            await _hostService.UpdateBusinessSettingsAsync(GetUserId(), request, cancellationToken);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    private Guid GetUserId()
    {
        return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
