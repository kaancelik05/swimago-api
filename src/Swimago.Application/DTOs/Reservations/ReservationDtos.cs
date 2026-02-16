using Swimago.Application.DTOs.Common;
using Swimago.Domain.Enums;

namespace Swimago.Application.DTOs.Reservations;

/// <summary>
/// Create reservation request with full guest and selection details
/// </summary>
public record CreateReservationRequest(
    Guid ListingId,
    BookingType BookingType,
    DateTime StartTime,
    DateTime EndTime,
    int GuestCount,
    string? SpecialRequests,
    // New optional fields for enhanced functionality
    VenueType? VenueType = null,
    GuestDetailsDto? Guests = null,
    ReservationSelectionsDto? Selections = null,
    Guid? PaymentMethodId = null
);

/// <summary>
/// Simple reservation response for backward compatibility
/// </summary>
public record ReservationResponse(
    Guid Id,
    Guid ListingId,
    Guid GuestId,
    string ListingTitle,
    string VenueType,
    string ConfirmationNumber,
    DateTime StartTime,
    DateTime EndTime,
    int GuestCount,
    decimal TotalPrice,
    decimal FinalPrice,
    string Currency,
    string Status,
    string BookingType,
    DateTime CreatedAt,
    string? SpecialRequests,
    PaymentResponse? Payment
);

/// <summary>
/// Payment info for reservation response
/// </summary>
public record PaymentResponse(
    Guid Id,
    decimal Amount,
    string Currency,
    string Status,
    string? PaymentIntentId
);

/// <summary>
/// Guest details for reservation
/// </summary>
public record GuestDetailsDto(
    int Adults,
    int Children,
    int? Infants
);

/// <summary>
/// Optional selections for reservation
/// </summary>
public record ReservationSelectionsDto(
    bool? Breakfast,
    bool? Lunch,
    bool? Towels,
    bool? Sunbeds,
    bool? Parking,
    Dictionary<string, object>? CustomSelections
);

/// <summary>
/// Reservation list response with counts by status
/// </summary>
public record ReservationListResponse(
    IEnumerable<ReservationListItemDto> Reservations,
    ReservationCountsDto Counts,
    int TotalCount
);

/// <summary>
/// Status counts for reservations
/// </summary>
public record ReservationCountsDto(
    int Total,
    int Pending,
    int Confirmed,
    int Completed,
    int Cancelled
);

/// <summary>
/// Reservation list item (summary view)
/// </summary>
public record ReservationListItemDto(
    Guid Id,
    string ConfirmationNumber,
    VenueType VenueType,
    string VenueName,
    string? VenueImageUrl,
    string? VenueCity,
    DateTime StartTime,
    DateTime EndTime,
    int GuestCount,
    decimal TotalPrice,
    string Currency,
    ReservationStatus Status,
    DateTime CreatedAt
);

/// <summary>
/// Full reservation response
/// </summary>
public record ReservationDetailResponse(
    Guid Id,
    string ConfirmationNumber,
    Guid ListingId,
    VenueType VenueType,
    ReservationVenueDto Venue,
    GuestDetailsDto Guests,
    ReservationSelectionsDto? Selections,
    DateTime StartTime,
    DateTime EndTime,
    ReservationPriceBreakdownDto PriceBreakdown,
    decimal TotalPrice,
    decimal FinalPrice,
    string Currency,
    ReservationStatus Status,
    string? CheckInCode,
    string? SpecialRequests,
    DateTime CreatedAt,
    ReservationPaymentDto? Payment,
    bool CanCancel,
    bool CanReview,
    DateTime? CancelDeadline
);

/// <summary>
/// Venue info in reservation
/// </summary>
public record ReservationVenueDto(
    Guid Id,
    string Name,
    string? Slug,
    string? ImageUrl,
    string? City,
    string? Country,
    HostInfoSummaryDto Host
);

/// <summary>
/// Host summary in reservation
/// </summary>
public record HostInfoSummaryDto(
    Guid Id,
    string Name,
    string? Avatar,
    string? PhoneNumber
);

/// <summary>
/// Price breakdown for reservation
/// </summary>
public record ReservationPriceBreakdownDto(
    decimal BasePrice,
    decimal? SelectionsTotal,
    decimal? ServiceFee,
    decimal? Discount,
    decimal Total
);

/// <summary>
/// Payment info in reservation
/// </summary>
public record ReservationPaymentDto(
    Guid Id,
    decimal Amount,
    string Currency,
    PaymentStatus Status,
    string? PaymentMethodLast4,
    string? PaymentMethodBrand,
    DateTime CreatedAt,
    DateTime? PaidAt
);

/// <summary>
/// Update reservation request
/// </summary>
public record UpdateReservationRequest(
    DateTime? StartTime,
    DateTime? EndTime,
    GuestDetailsDto? Guests,
    ReservationSelectionsDto? Selections,
    string? SpecialRequests
);

/// <summary>
/// Cancel reservation request
/// </summary>
public record CancelReservationRequest(
    string? Reason
);

/// <summary>
/// Check-in response
/// </summary>
public record CheckInResponse(
    bool Success,
    string? CheckInCode,
    string? Message,
    DateTime? CheckedInAt
);

/// <summary>
/// Create review for reservation
/// </summary>
public record CreateReservationReviewRequest(
    int Rating,
    string Comment,
    Dictionary<string, int>? CategoryRatings
);

/// <summary>
/// Submit review request
/// </summary>
public record SubmitReviewRequest(
    int Rating,
    string Comment
);
