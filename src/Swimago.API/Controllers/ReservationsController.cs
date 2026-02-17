using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swimago.API.Authorization;
using Swimago.Application.DTOs.Reservations;
using Swimago.Application.Interfaces;
using Swimago.Domain.Enums;
using System.Security.Claims;

namespace Swimago.API.Controllers;

/// <summary>
/// Reservation management endpoints for booking beaches, pools, and boat tours
/// </summary>
[Authorize(Policy = AuthorizationPolicies.CustomerOnly)]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _reservationService;
    private readonly ILogger<ReservationsController> _logger;

    public ReservationsController(
        IReservationService reservationService,
        ILogger<ReservationsController> logger)
    {
        _reservationService = reservationService;
        _logger = logger;
    }

    /// <summary>
    /// Get all reservations for the authenticated user with optional filters
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ReservationListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] ReservationStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        _logger.LogInformation("Fetching reservations for user {UserId}, status={Status}", userId, status);
        
        var allReservations = await _reservationService.GetUserReservationsAsync(userId, cancellationToken);
        
        var statusPending = ReservationStatus.Pending.ToString();
        var statusConfirmed = ReservationStatus.Confirmed.ToString();
        var statusCompleted = ReservationStatus.Completed.ToString();
        var statusCancelled = ReservationStatus.Cancelled.ToString();

        var statusCounts = new
        {
            total = allReservations.Count(),
            pending = allReservations.Count(r => r.Status == statusPending),
            confirmed = allReservations.Count(r => r.Status == statusConfirmed),
            completed = allReservations.Count(r => r.Status == statusCompleted),
            cancelled = allReservations.Count(r => r.Status == statusCancelled)
        };

        if (status.HasValue)
        {
            var filterStatus = status.Value.ToString();
            allReservations = allReservations.Where(r => r.Status == filterStatus);
        }

        var total = allReservations.Count();
        
        var mappedItems = allReservations
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new ReservationListItemDto(
                r.Id,
                r.ConfirmationNumber,
                Enum.Parse<VenueType>(r.VenueType), 
                r.ListingTitle, 
                null, // VenueImageUrl - need to be added to ReservationResponse if needed
                null, // VenueCity - need to be added
                r.StartTime,
                r.EndTime,
                r.GuestCount,
                r.TotalPrice,
                r.Currency,
                Enum.Parse<ReservationStatus>(r.Status),
                r.CreatedAt
            ))
            .ToList();

        var counts = new ReservationCountsDto(
            statusCounts.total,
            statusCounts.pending,
            statusCounts.confirmed,
            statusCounts.completed,
            statusCounts.cancelled
        );

        return Ok(new ReservationListResponse(mappedItems, counts, total));
    }

    /// <summary>
    /// Create a new reservation
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ReservationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreateReservationRequest request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        _logger.LogInformation("Creating reservation for user {UserId}, listing {ListingId}", userId, request.ListingId);

        try
        {
            var response = await _reservationService.CreateReservationAsync(userId, request, cancellationToken);
            
            _logger.LogInformation("Reservation created successfully: {ReservationId}", response.Id);
            
            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Reservation creation failed: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Listing not found: {Message}", ex.Message);
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get reservation details by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ReservationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var response = await _reservationService.GetReservationByIdAsync(id, cancellationToken);
        
        if (response == null)
            return NotFound(new { error = "Rezervasyon bulunamadı" });

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        if (response.GuestId != userId)
        {
            _logger.LogWarning("User {UserId} attempted to access reservation {ReservationId}", userId, id);
            return Forbid();
        }

        return Ok(response);
    }

    /// <summary>
    /// Update a reservation
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ReservationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateReservationRequest request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        _logger.LogInformation("Updating reservation {ReservationId} for user {UserId}", id, userId);

        try
        {
            var existingReservation = await _reservationService.GetReservationByIdAsync(id, cancellationToken);
            
            if (existingReservation == null)
                return NotFound(new { error = "Rezervasyon bulunamadı" });

            if (existingReservation.GuestId != userId)
                return Forbid();

            // TODO: Implement update logic in service
            // For now, return the existing reservation
            return Ok(existingReservation);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Reservation update failed: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Cancel a reservation
    /// </summary>
    [HttpPost("{id}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        _logger.LogInformation("User {UserId} attempting to cancel reservation {ReservationId}", userId, id);

        try
        {
            await _reservationService.CancelReservationAsync(id, userId, cancellationToken);
            
            _logger.LogInformation("Reservation {ReservationId} cancelled successfully", id);
            
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Cancellation failed: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            _logger.LogWarning("Unauthorized cancellation attempt by user {UserId}", userId);
            return Forbid();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "Rezervasyon bulunamadı" });
        }
    }

    /// <summary>
    /// Check-in to a reservation
    /// </summary>
    [HttpPost("{id}/check-in")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CheckIn(Guid id, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        _logger.LogInformation("User {UserId} checking in to reservation {ReservationId}", userId, id);

        try
        {
            var reservation = await _reservationService.GetReservationByIdAsync(id, cancellationToken);
            
            if (reservation == null)
                return NotFound(new { error = "Rezervasyon bulunamadı" });

            if (reservation.GuestId != userId)
                return Forbid();

            // Use string comparison for status
            if (reservation.Status != ReservationStatus.Confirmed.ToString())
                return BadRequest(new { error = "Bu rezervasyon için check-in yapılamaz" });

            // TODO: Implement check-in logic in service
            _logger.LogInformation("Check-in successful for reservation {ReservationId}", id);
            
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Check-in failed: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Submit a review for a completed reservation
    /// </summary>
    [HttpPost("{id}/review")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SubmitReview(Guid id, [FromBody] SubmitReviewRequest request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        _logger.LogInformation("User {UserId} submitting review for reservation {ReservationId}", userId, id);

        try
        {
            var reservation = await _reservationService.GetReservationByIdAsync(id, cancellationToken);
            
            if (reservation == null)
                return NotFound(new { error = "Rezervasyon bulunamadı" });

            if (reservation.GuestId != userId)
                return Forbid();

            // Use string comparison for status
            if (reservation.Status != ReservationStatus.Completed.ToString())
                return BadRequest(new { error = "Sadece tamamlanmış rezervasyonlar için değerlendirme yapılabilir" });

            if (request.Rating < 1 || request.Rating > 5)
                return BadRequest(new { error = "Puan 1-5 arasında olmalıdır" });

            // TODO: Create review via service
            _logger.LogInformation("Review submitted for reservation {ReservationId}, rating: {Rating}", id, request.Rating);
            
            return Created($"/api/reviews/{Guid.NewGuid()}", new { message = "Değerlendirmeniz başarıyla gönderildi" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Review submission failed: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Check availability for a listing (public endpoint)
    /// </summary>
    [AllowAnonymous]
    [HttpGet("check-availability")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CheckAvailability(
        [FromQuery] Guid listingId, 
        [FromQuery] DateTime startTime, 
        [FromQuery] DateTime endTime, 
        CancellationToken cancellationToken)
    {
        if (startTime >= endTime)
            return BadRequest(new { error = "Başlangıç tarihi, bitiş tarihinden önce olmalıdır" });

        if (startTime < DateTime.UtcNow)
            return BadRequest(new { error = "Geçmiş tarih için müsaitlik sorgulanamaz" });

        var isAvailable = await _reservationService.CheckAvailabilityAsync(listingId, startTime, endTime, cancellationToken);
        
        return Ok(new { listingId, startTime, endTime, isAvailable });
    }
}
