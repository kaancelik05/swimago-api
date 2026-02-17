using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swimago.API.Authorization;
using Swimago.Application.DTOs.Admin;
using Swimago.Application.Interfaces;
using Swimago.Domain.Enums;
using System.Security.Claims;

namespace Swimago.API.Controllers;

[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[ApiController]
[Route("api/admin")]
[Produces("application/json")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        IAdminService adminService,
        ILogger<AdminController> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }

    /// <summary>
    /// Get admin dashboard stats
    /// </summary>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(AdminDashboardResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboard(CancellationToken cancellationToken)
    {
        var result = await _adminService.GetDashboardAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get users list
    /// </summary>
    [HttpGet("users")]
    [ProducesResponseType(typeof(AdminUserListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsers(
        [FromQuery] Role? role, 
        [FromQuery] UserStatus? status, 
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _adminService.GetUsersAsync(role, status, search, page, pageSize, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get user details
    /// </summary>
    [HttpGet("users/{id}")]
    [ProducesResponseType(typeof(AdminUserDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUser(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _adminService.GetUserAsync(id, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "Kullanıcı bulunamadı" });
        }
    }

    /// <summary>
    /// Update user status
    /// </summary>
    [HttpPut("users/{id}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUserStatus(Guid id, [FromBody] UpdateUserStatusRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await _adminService.UpdateUserStatusAsync(id, request, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "Kullanıcı bulunamadı" });
        }
    }

    /// <summary>
    /// Update user role
    /// </summary>
    [HttpPut("users/{id}/role")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUserRole(Guid id, [FromBody] UpdateUserRoleRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await _adminService.UpdateUserRoleAsync(id, request, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "Kullanıcı bulunamadı" });
        }
    }

    /// <summary>
    /// Get host applications
    /// </summary>
    [HttpGet("host-applications")]
    [ProducesResponseType(typeof(HostApplicationListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHostApplications(CancellationToken cancellationToken)
    {
        var result = await _adminService.GetHostApplicationsAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Reject host application
    /// </summary>
    [HttpPost("host-applications/{userId}/reject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RejectHostApplication(Guid userId, [FromBody] RejectHostRequest request, CancellationToken cancellationToken)
    {
        await _adminService.RejectHostApplicationAsync(userId, request, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Get listings
    /// </summary>
    [HttpGet("listings")]
    [ProducesResponseType(typeof(AdminListingListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetListings(
        [FromQuery] ListingStatus? status,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _adminService.GetListingsAsync(status, search, page, pageSize, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Approve listing
    /// </summary>
    [HttpPost("listings/{id}/approve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ApproveListing(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _adminService.ApproveListingAsync(id, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "İlan bulunamadı" });
        }
    }

    /// <summary>
    /// Reject listing
    /// </summary>
    [HttpPost("listings/{id}/reject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RejectListing(Guid id, [FromBody] RejectListingRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await _adminService.RejectListingAsync(id, request, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "İlan bulunamadı" });
        }
    }

    /// <summary>
    /// Get reports/analytics
    /// </summary>
    [HttpGet("reports")]
    [ProducesResponseType(typeof(AdminReportResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReports(
        [FromQuery] DateTime start,
        [FromQuery] DateTime end,
        CancellationToken cancellationToken)
    {
        var result = await _adminService.GetReportsAsync(start, end, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get cities
    /// </summary>
    [HttpGet("cities")]
    [ProducesResponseType(typeof(CityListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCities(CancellationToken cancellationToken)
    {
        var result = await _adminService.GetCitiesAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Create city
    /// </summary>
    [HttpPost("cities")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> CreateCity([FromBody] CreateCityRequest request, CancellationToken cancellationToken)
    {
        await _adminService.CreateCityAsync(request, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Get amenities
    /// </summary>
    [HttpGet("amenities")]
    [ProducesResponseType(typeof(AmenityListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAmenities(CancellationToken cancellationToken)
    {
        var result = await _adminService.GetAmenitiesAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Create amenity
    /// </summary>
    [HttpPost("amenities")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> CreateAmenity([FromBody] CreateAmenityRequest request, CancellationToken cancellationToken)
    {
        await _adminService.CreateAmenityAsync(request, cancellationToken);
        return NoContent();
    }
}
