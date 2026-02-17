using Swimago.Application.DTOs.Common;
using Swimago.Domain.Enums;

namespace Swimago.Application.DTOs.Destinations;

/// <summary>
/// Destination list response (cities with beach/pool venues)
/// </summary>
public record DestinationListResponse(
    IEnumerable<DestinationItemDto> Destinations,
    int TotalCount
);

/// <summary>
/// Single destination item in list
/// </summary>
public record DestinationItemDto(
    Guid Id,
    string Slug,
    string Name,
    string? Country,
    string? ImageUrl,
    int SpotCount,
    string Type,
    decimal? MinPrice,
    decimal? MaxPrice,
    decimal? AverageRating,
    bool IsFeatured
);

/// <summary>
/// Destination detail response with spots
/// </summary>
public record DestinationDetailResponse(
    Guid Id,
    string Slug,
    string Name,
    string? Description,
    string? Country,
    string? ImageUrl,
    decimal? Latitude,
    decimal? Longitude,
    IEnumerable<SpotListItemDto> Spots,
    IEnumerable<string>? Tags,
    decimal? AverageRating,
    int ReviewCount
);

/// <summary>
/// Customer destination detail page response
/// </summary>
public record DestinationPageDetailResponse(
    Guid Id,
    string Slug,
    string Type,
    DestinationHeroDto Hero,
    DestinationOverviewDto Overview,
    IEnumerable<DestinationFeatureItemDto> Features,
    IEnumerable<DestinationSpotItemDto> Spots,
    DestinationCtaDto Cta
);

public record DestinationHeroDto(
    string Title,
    string? Subtitle,
    string Location,
    string? ImageUrl,
    int SpotCount
);

public record DestinationOverviewDto(
    string? Description,
    string? AvgWaterTemp,
    int? SunnyDaysPerYear,
    string? MapImageUrl
);

public record DestinationFeatureItemDto(
    string Icon,
    string Title,
    string Description
);

public record DestinationSpotItemDto(
    Guid Id,
    string Slug,
    string Name,
    string? Location,
    string? ImageUrl,
    decimal Rating,
    int ReviewCount,
    decimal Price,
    string Currency,
    string PriceUnit
);

public record DestinationCtaDto(
    string Title,
    string Description,
    string ButtonText,
    string? BackgroundImageUrl
);

/// <summary>
/// Spot list item (beach/pool in destination)
/// </summary>
public record SpotListItemDto(
    Guid Id,
    string Slug,
    string Name,
    VenueType VenueType,
    string? ImageUrl,
    decimal BasePricePerDay,
    string Currency,
    decimal Rating,
    int ReviewCount,
    int MaxGuestCount,
    bool IsFeatured,
    IEnumerable<string>? Amenities
);

/// <summary>
/// Spot detail response (full beach/pool info)
/// </summary>
public record SpotDetailResponse(
    Guid Id,
    string Slug,
    MultiLanguageDto Name,
    MultiLanguageDto? Description,
    VenueType VenueType,
    string Status,
    string? City,
    string? Country,
    decimal Latitude,
    decimal Longitude,
    decimal BasePricePerDay,
    decimal? PriceRangeMin,
    decimal? PriceRangeMax,
    string Currency,
    int MaxGuestCount,
    decimal Rating,
    int ReviewCount,
    bool IsFeatured,
    IEnumerable<SpotImageDto> Images,
    IEnumerable<SpotAmenityDto> Amenities,
    MultiLanguageDto? Conditions,
    HostInfoDto Host,
    IEnumerable<AvailabilitySlotDto>? AvailableSlots
);

/// <summary>
/// Spot image
/// </summary>
public record SpotImageDto(
    Guid Id,
    string Url,
    bool IsCover,
    int DisplayOrder,
    string? AltText
);

/// <summary>
/// Spot amenity
/// </summary>
public record SpotAmenityDto(
    Guid Id,
    string Name,
    string? Icon,
    string? Category
);

/// <summary>
/// Host info in spot detail
/// </summary>
public record HostInfoDto(
    Guid Id,
    string Name,
    string? Avatar,
    DateTime MemberSince,
    int ListingCount,
    decimal AverageRating,
    int ResponseRate,
    string? ResponseTime
);

/// <summary>
/// Available time slots
/// </summary>
public record AvailabilitySlotDto(
    DateTime Date,
    bool IsAvailable,
    decimal? Price
);

/// <summary>
/// Explore/map view response
/// </summary>
public record ExploreResponse(
    IEnumerable<ExploreMarkerDto> Markers,
    ExploreMapBoundsDto Bounds
);

/// <summary>
/// Map marker for explore view
/// </summary>
public record ExploreMarkerDto(
    Guid Id,
    string Slug,
    string Name,
    VenueType VenueType,
    decimal Latitude,
    decimal Longitude,
    decimal Price,
    string Currency,
    decimal Rating,
    string? ThumbnailUrl
);

/// <summary>
/// Map bounds for explore view
/// </summary>
public record ExploreMapBoundsDto(
    decimal NorthEastLat,
    decimal NorthEastLng,
    decimal SouthWestLat,
    decimal SouthWestLng
);
