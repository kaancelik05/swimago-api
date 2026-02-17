namespace Swimago.Application.DTOs.Destinations;

public record CustomerSpotDetailResponse(
    Guid Id,
    string Slug,
    string Type,
    SpotHeaderDto Header,
    IEnumerable<SpotGalleryItemDto> Gallery,
    IEnumerable<SpotConditionItemDto> Conditions,
    string? Description,
    IEnumerable<SpotAmenityAvailabilityDto> Amenities,
    SpotLocationDto Location,
    SpotReviewsPreviewDto ReviewsPreview,
    SpotBookingDefaultsDto BookingDefaults
);

public record SpotHeaderDto(
    string Title,
    decimal Rating,
    int ReviewCount,
    string Location,
    IEnumerable<SpotBreadcrumbDto> Breadcrumbs
);

public record SpotBreadcrumbDto(
    string Label,
    string? Link
);

public record SpotGalleryItemDto(
    string Url,
    string? Alt,
    bool IsPrimary
);

public record SpotConditionItemDto(
    string Icon,
    string Label,
    string Value
);

public record SpotAmenityAvailabilityDto(
    string Icon,
    string Label,
    bool Available
);

public record SpotLocationDto(
    string Name,
    string? Subtitle,
    decimal Latitude,
    decimal Longitude,
    string? MapImageUrl
);

public record SpotReviewsPreviewDto(
    decimal OverallRating,
    int TotalReviews,
    IEnumerable<SpotReviewBreakdownDto> Breakdown,
    IEnumerable<SpotReviewCategoryDto> Categories,
    IEnumerable<SpotReviewPreviewItemDto> Reviews
);

public record SpotReviewBreakdownDto(
    int Stars,
    int Percentage
);

public record SpotReviewCategoryDto(
    string Label,
    decimal Score
);

public record SpotReviewPreviewItemDto(
    Guid Id,
    string? AvatarUrl,
    string Name,
    DateTime Date,
    string Text
);

public record SpotBookingDefaultsDto(
    decimal Price,
    string Currency,
    string PriceUnit,
    DateTime DefaultDate,
    int DefaultGuests,
    IEnumerable<SpotLineItemDto> LineItems,
    decimal Total,
    string? RareFindMessage
);

public record SpotLineItemDto(
    string Label,
    decimal Amount
);

public record SpotQuoteRequest(
    DateTime Date,
    SpotQuoteGuestsDto Guests,
    SpotQuoteSelectionsDto? Selections
);

public record SpotQuoteGuestsDto(
    int Adults,
    int Children
);

public record SpotQuoteSelectionsDto(
    IEnumerable<string>? SelectedAmenities
);

public record SpotQuoteResponse(
    string Currency,
    IEnumerable<SpotLineItemDto> LineItems,
    decimal Total,
    bool IsAvailable,
    string? UnavailableReason
);
