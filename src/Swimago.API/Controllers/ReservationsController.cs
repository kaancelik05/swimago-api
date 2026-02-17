using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swimago.API.Authorization;
using Swimago.Application.DTOs.Reservations;
using Swimago.Application.Interfaces;
using Swimago.Domain.Entities;
using Swimago.Domain.Enums;
using Swimago.Domain.Interfaces;
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
    private readonly IReservationRepository _reservationRepository;
    private readonly IReviewRepository _reviewRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ReservationsController> _logger;

    public ReservationsController(
        IReservationService reservationService,
        IReservationRepository reservationRepository,
        IReviewRepository reviewRepository,
        IUnitOfWork unitOfWork,
        ILogger<ReservationsController> logger)
    {
        _reservationService = reservationService;
        _reservationRepository = reservationRepository;
        _reviewRepository = reviewRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Get all reservations for the authenticated user with optional filters
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(CustomerReservationListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] ReservationStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var safePage = Math.Max(1, page);
        var safePageSize = Math.Clamp(pageSize, 1, 100);

        _logger.LogInformation("Fetching reservations for user {UserId}, status={Status}", userId, status);

        var reservations = (await _reservationRepository.GetByGuestIdAndStatusAsync(userId, status, cancellationToken))
            .OrderByDescending(x => x.StartTime)
            .ToList();

        var totalCount = reservations.Count;
        var items = reservations
            .Skip((safePage - 1) * safePageSize)
            .Take(safePageSize)
            .Select(MapReservationListItem)
            .ToList();

        var totalPages = (int)Math.Ceiling(totalCount / (double)safePageSize);

        return Ok(new CustomerReservationListResponse(
            Items: items,
            Page: safePage,
            PageSize: safePageSize,
            TotalCount: totalCount,
            TotalPages: totalPages,
            HasPrevious: safePage > 1,
            HasNext: safePage < totalPages
        ));
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

        var reservation = await _reservationRepository.GetWithDetailsAsync(id, cancellationToken);
        if (reservation == null)
            return NotFound(new { error = "Rezervasyon bulunamadı" });

        if (reservation.GuestId != userId)
            return Forbid();

        if (reservation.Status is ReservationStatus.Cancelled or ReservationStatus.Completed)
            return BadRequest(new { error = "Bu rezervasyon güncellenemez" });

        if (request.StartTime.HasValue || request.EndTime.HasValue)
        {
            var start = request.StartTime ?? reservation.StartTime;
            var end = request.EndTime ?? reservation.EndTime;

            if (start >= end)
                return BadRequest(new { error = "Başlangıç tarihi bitiş tarihinden önce olmalıdır" });

            var hasOverlap = await _reservationRepository.HasOverlappingReservationsAsync(
                reservation.ListingId,
                start,
                end,
                reservation.Id,
                cancellationToken);

            if (hasOverlap)
                return BadRequest(new { error = "Seçilen tarih aralığı dolu" });

            reservation.StartTime = start;
            reservation.EndTime = end;
        }

        if (request.Guests != null)
        {
            var totalGuests = Math.Max(1, request.Guests.Adults + request.Guests.Children + (request.Guests.Infants ?? 0));
            reservation.GuestCount = totalGuests;
            reservation.Guests = new GuestDetails
            {
                Adults = request.Guests.Adults,
                Children = request.Guests.Children,
                Seniors = 0
            };
        }

        if (!string.IsNullOrWhiteSpace(request.SpecialRequests))
        {
            reservation.SpecialRequests = new Dictionary<string, string>
            {
                ["tr"] = request.SpecialRequests
            };
        }

        await _reservationRepository.UpdateAsync(reservation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updated = await _reservationService.GetReservationByIdAsync(id, cancellationToken);
        return Ok(updated);
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

        var reservation = await _reservationRepository.GetByIdAsync(id, cancellationToken);

        if (reservation == null)
            return NotFound(new { error = "Rezervasyon bulunamadı" });

        if (reservation.GuestId != userId)
            return Forbid();

        if (reservation.Status != ReservationStatus.Confirmed)
            return BadRequest(new { error = "Bu rezervasyon için check-in yapılamaz" });

        reservation.CheckedInAt = DateTime.UtcNow;
        reservation.Status = ReservationStatus.Completed;

        await _reservationRepository.UpdateAsync(reservation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return NoContent();
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

        var reservation = await _reservationRepository.GetWithDetailsAsync(id, cancellationToken);

        if (reservation == null)
            return NotFound(new { error = "Rezervasyon bulunamadı" });

        if (reservation.GuestId != userId)
            return Forbid();

        if (reservation.Status != ReservationStatus.Completed)
            return BadRequest(new { error = "Sadece tamamlanmış rezervasyonlar için değerlendirme yapılabilir" });

        if (request.Rating < 1 || request.Rating > 5)
            return BadRequest(new { error = "Puan 1-5 arasında olmalıdır" });

        if (reservation.Review != null)
            return BadRequest(new { error = "Bu rezervasyon için zaten değerlendirme yapılmış" });

        var review = new Review
        {
            Id = Guid.NewGuid(),
            ReservationId = reservation.Id,
            ListingId = reservation.ListingId,
            GuestId = reservation.GuestId,
            Rating = request.Rating,
            Text = request.Comment,
            CreatedAt = DateTime.UtcNow,
            IsVerified = true
        };

        await _reviewRepository.AddAsync(review, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Created($"/api/reviews/{review.Id}", new
        {
            id = review.Id,
            message = "Değerlendirmeniz başarıyla gönderildi"
        });
    }

    /// <summary>
    /// Create payment intent for reservation
    /// </summary>
    [HttpPost("{id}/payment-intent")]
    [ProducesResponseType(typeof(ReservationPaymentIntentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreatePaymentIntent(Guid id, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var reservation = await _reservationRepository.GetWithDetailsAsync(id, cancellationToken);
        if (reservation == null)
            return NotFound(new { error = "Rezervasyon bulunamadı" });

        if (reservation.GuestId != userId)
            return Forbid();

        reservation.Payment ??= new ReservationPayment
        {
            Id = Guid.NewGuid(),
            ReservationId = reservation.Id,
            Amount = reservation.FinalPrice,
            Currency = reservation.Currency,
            Status = PaymentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _reservationRepository.UpdateAsync(reservation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(new ReservationPaymentIntentResponse(
            ReservationId: reservation.Id,
            PaymentId: reservation.Payment.Id,
            Amount: reservation.Payment.Amount,
            Currency: reservation.Payment.Currency,
            Status: reservation.Payment.Status.ToString().ToLowerInvariant()
        ));
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

    private static CustomerReservationListItemDto MapReservationListItem(Reservation reservation)
    {
        var listing = reservation.Listing;
        var venueName = listing == null
            ? reservation.ConfirmationNumber ?? "Reservation"
            : GetLocalizedText(listing.Title);

        var location = listing == null
            ? null
            : (string.IsNullOrWhiteSpace(listing.Country)
                ? listing.City
                : $"{listing.City}, {listing.Country}");

        var imageUrl = listing?.Images.FirstOrDefault(i => i.IsCover)?.Url
            ?? listing?.Images.FirstOrDefault()?.Url;

        var selection = reservation.Selections == null
            ? null
            : $"{reservation.Selections.Sunbeds} Sunbeds, {reservation.Selections.Cabanas} Cabana";

        return new CustomerReservationListItemDto(
            Id: reservation.Id,
            VenueName: venueName,
            Location: location,
            ImageUrl: imageUrl,
            Date: reservation.StartTime.Date,
            Time: $"{reservation.StartTime:HH:mm} - {reservation.EndTime:HH:mm}",
            Selection: selection,
            Price: reservation.TotalPrice,
            Status: reservation.Status.ToString().ToLowerInvariant(),
            Guests: $"{reservation.GuestCount} Adults"
        );
    }

    private static string GetLocalizedText(Dictionary<string, string>? values)
    {
        if (values == null || values.Count == 0)
        {
            return string.Empty;
        }

        if (values.TryGetValue("tr", out var tr) && !string.IsNullOrWhiteSpace(tr))
        {
            return tr;
        }

        if (values.TryGetValue("en", out var en) && !string.IsNullOrWhiteSpace(en))
        {
            return en;
        }

        return values.Values.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)) ?? string.Empty;
    }
}
