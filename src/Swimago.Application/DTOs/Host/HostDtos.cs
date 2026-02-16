using Swimago.Application.DTOs.Common;
using Swimago.Application.DTOs.Destinations;
using Swimago.Domain.Enums;

namespace Swimago.Application.DTOs.Host;

/// <summary>
/// Host dashboard response
/// </summary>
public record HostDashboardResponse(
    HostStatsDto Stats,
    IEnumerable<RecentReservationDto> RecentReservations,
    IEnumerable<RecentReviewDto> RecentReviews,
    HostEarningsDto Earnings
);

/// <summary>
/// Host statistics
/// </summary>
public record HostStatsDto(
    int TotalListings,
    int ActiveListings,
    int PendingListings,
    int TotalReservations,
    int PendingReservations,
    int TodayReservations,
    decimal AverageRating,
    int TotalReviews
);

/// <summary>
/// Recent reservation summary for dashboard
/// </summary>
public record RecentReservationDto(
    Guid Id,
    string ConfirmationNumber,
    string GuestName,
    string VenueName,
    DateTime StartTime,
    DateTime EndTime,
    decimal TotalPrice,
    ReservationStatus Status
);

/// <summary>
/// Recent review for dashboard
/// </summary>
public record RecentReviewDto(
    Guid Id,
    string GuestName,
    string? GuestAvatar,
    string VenueName,
    int Rating,
    string? Comment,
    DateTime CreatedAt,
    bool HasHostResponse
);

/// <summary>
/// Host earnings summary
/// </summary>
public record HostEarningsDto(
    decimal TotalEarnings,
    decimal ThisMonthEarnings,
    decimal LastMonthEarnings,
    decimal PendingPayouts,
    string Currency
);

/// <summary>
/// Host listings list response
/// </summary>
public record HostListingListResponse(
    IEnumerable<HostListingItemDto> Listings,
    int TotalCount,
    int ActiveCount,
    int PendingCount,
    int InactiveCount
);

/// <summary>
/// Host listing item
/// </summary>
public record HostListingItemDto(
    Guid Id,
    string Name,
    string? Slug,
    VenueType VenueType,
    string? ImageUrl,
    ListingStatus Status,
    decimal BasePricePerDay,
    string Currency,
    decimal Rating,
    int ReviewCount,
    int ReservationCount,
    decimal TotalEarnings,
    DateTime CreatedAt
);

/// <summary>
/// Host reservations list response
/// </summary>
public record HostReservationListResponse(
    IEnumerable<HostReservationItemDto> Reservations,
    HostReservationCountsDto Counts,
    int TotalCount
);

/// <summary>
/// Host reservation counts by status
/// </summary>
public record HostReservationCountsDto(
    int Total,
    int Pending,
    int Confirmed,
    int CheckedIn,
    int Completed,
    int Cancelled
);

/// <summary>
/// Host reservation item
/// </summary>
public record HostReservationItemDto(
    Guid Id,
    string ConfirmationNumber,
    Guid ListingId,
    string ListingName,
    Guid GuestId,
    string GuestName,
    string? GuestAvatar,
    string? GuestPhone,
    DateTime StartTime,
    DateTime EndTime,
    int GuestCount,
    decimal TotalPrice,
    string Currency,
    ReservationStatus Status,
    string? SpecialRequests,
    DateTime CreatedAt
);

/// <summary>
/// Update reservation status request
/// </summary>
public record UpdateReservationStatusRequest(
    ReservationStatus Status,
    string? Message
);

/// <summary>
/// Host calendar response
/// </summary>
public record HostCalendarResponse(
    Guid ListingId,
    string ListingName,
    IEnumerable<CalendarDayDto> Days
);

/// <summary>
/// Calendar day info
/// </summary>
public record CalendarDayDto(
    DateTime Date,
    bool IsAvailable,
    decimal? Price,
    CalendarReservationDto? Reservation
);

/// <summary>
/// Reservation info in calendar
/// </summary>
public record CalendarReservationDto(
    Guid Id,
    string GuestName,
    int GuestCount,
    ReservationStatus Status
);

/// <summary>
/// Update calendar request
/// </summary>
public record UpdateCalendarRequest(
    Guid ListingId,
    IEnumerable<CalendarUpdateDto> Updates
);

/// <summary>
/// Single calendar day update
/// </summary>
public record CalendarUpdateDto(
    DateTime Date,
    bool IsAvailable,
    decimal? Price
);

/// <summary>
/// Host analytics response
/// </summary>
public record HostAnalyticsResponse(
    AnalyticsPeriodDto CurrentPeriod,
    AnalyticsPeriodDto PreviousPeriod,
    IEnumerable<DailyAnalyticsDto> DailyData,
    IEnumerable<ListingAnalyticsDto> ListingPerformance
);

/// <summary>
/// Analytics for a period
/// </summary>
public record AnalyticsPeriodDto(
    decimal TotalRevenue,
    int TotalReservations,
    decimal AverageBookingValue,
    decimal OccupancyRate,
    decimal AverageRating
);

/// <summary>
/// Daily analytics data point
/// </summary>
public record DailyAnalyticsDto(
    DateTime Date,
    decimal Revenue,
    int Reservations,
    int PageViews
);

/// <summary>
/// Per-listing analytics
/// </summary>
public record ListingAnalyticsDto(
    Guid ListingId,
    string ListingName,
    decimal Revenue,
    int Reservations,
    decimal Rating,
    decimal OccupancyRate
);

/// <summary>
/// Update listing request
/// </summary>
public record UpdateListingRequest(
    string Name,
    string Description,
    int? Capacity,
    decimal Price,
    string? CheckInTime,
    string? CheckOutTime
);

/// <summary>
/// Update listing pricing request
/// </summary>
public record UpdatePricingRequest(
    decimal? BasePricePerDay,
    decimal? WeekendPriceMultiplier,
    IEnumerable<SeasonalPricingDto>? SeasonalPricing,
    IEnumerable<SpecialDatePricingDto>? SpecialDates
);

/// <summary>
/// Seasonal pricing configuration
/// </summary>
public record SeasonalPricingDto(
    DateTime StartDate,
    DateTime EndDate,
    decimal PriceMultiplier,
    string? Name
);

/// <summary>
/// Special date pricing
/// </summary>
public record SpecialDatePricingDto(
    DateTime Date,
    decimal Price,
    string? Description
);
