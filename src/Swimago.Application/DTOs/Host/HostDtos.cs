namespace Swimago.Application.DTOs.Host;

public record HostListingsResponse(
    IReadOnlyCollection<HostListingDto> Items,
    int TotalCount,
    int Page,
    int PageSize
);

public record HostListingDto(
    Guid Id,
    string Name,
    string Slug,
    string Type,
    string City,
    string ImageUrl,
    string Status,
    decimal Rating,
    int ReviewCount,
    int ReservationCount,
    decimal Revenue,
    decimal BasePrice,
    string Currency,
    int Capacity,
    IReadOnlyCollection<HostSeatingAreaDto> SeatingAreas,
    IReadOnlyCollection<string> Highlights,
    string? AvailabilityNotes
);

public record HostSeatingAreaDto(
    string Id,
    string Name,
    int Capacity,
    decimal PriceMultiplier,
    bool IsVip,
    decimal? MinSpend
);

public record UpsertHostListingRequest(
    string Name,
    string Type,
    string City,
    string Status,
    decimal BasePrice,
    string Currency,
    int Capacity,
    IReadOnlyCollection<string>? Highlights,
    IReadOnlyCollection<HostSeatingAreaDto>? SeatingAreas,
    string? AvailabilityNotes,
    string? ImageUrl
);

public record UpdateHostListingStatusRequest(string Status);

public record DashboardStatsDto(
    int TotalListings,
    int ActiveListings,
    int PendingReservations,
    int UpcomingReservations,
    decimal TotalRevenue,
    decimal MonthlyRevenue
);

public record HostReservationDto(
    Guid Id,
    Guid ListingId,
    string ListingName,
    string GuestName,
    string GuestPhone,
    string Date,
    string Time,
    int Guests,
    decimal TotalAmount,
    string Status,
    string Source,
    string? SpecialRequests,
    string CreatedAt
);

public record HostReservationsResponse(
    IReadOnlyCollection<HostReservationDto> Items,
    int TotalCount,
    int Page,
    int PageSize
);

public record UpdateHostReservationStatusRequest(string Status);

public record CreateManualReservationRequest(
    Guid ListingId,
    string GuestName,
    string GuestPhone,
    string Date,
    string Time,
    int Guests,
    decimal TotalAmount,
    string Source,
    string? SpecialRequests
);

public record HostInsightDto(
    string Id,
    string TitleKey,
    string DescriptionKey,
    IReadOnlyDictionary<string, object?>? DescriptionParams,
    string Level
);

public record CalendarDayDto(
    string Date,
    bool IsAvailable,
    int ReservationCount,
    decimal? CustomPrice
);

public record UpdateCalendarRequest(
    Guid ListingId,
    IReadOnlyCollection<CalendarUpdateDto> Updates
);

public record CalendarUpdateDto(
    string Date,
    bool IsAvailable,
    decimal? CustomPrice
);

public record HostAnalyticsDto(
    decimal TotalRevenue,
    decimal RevenueTrendPercent,
    int TotalReservations,
    decimal ReservationTrendPercent,
    decimal AverageRating,
    int ReviewCount,
    decimal OccupancyRate,
    IReadOnlyCollection<RevenuePointDto> RevenueSeries,
    IReadOnlyCollection<TopListingMetricDto> TopListings,
    IReadOnlyCollection<SourceBreakdownDto> SourceBreakdown,
    decimal NoShowRate,
    decimal CancellationRate
);

public record RevenuePointDto(string Label, decimal Amount);

public record TopListingMetricDto(
    Guid ListingId,
    string Name,
    decimal Revenue,
    int Bookings,
    decimal OccupancyRate
);

public record SourceBreakdownDto(string Source, int Count);

public record BusinessSettingsDto(
    bool AutoConfirmReservations,
    bool AllowSameDayBookings,
    int MinimumNoticeHours,
    int CancellationWindowHours,
    bool DynamicPricingEnabled,
    bool SmartOverbookingProtection,
    bool WhatsappNotifications,
    bool EmailNotifications
);

