using Swimago.Domain.Enums;

namespace Swimago.Application.DTOs.Favorites;

/// <summary>
/// Favorites list response
/// </summary>
public record FavoriteListResponse(
    IEnumerable<FavoriteItemDto> Favorites,
    int TotalCount
);

/// <summary>
/// Single favorite item
/// </summary>
public record FavoriteItemDto(
    Guid Id,
    Guid VenueId,
    VenueType VenueType,
    string VenueName,
    string? VenueSlug,
    string? VenueImageUrl,
    string? VenueCity,
    decimal? DistanceKm,
    decimal? VenuePrice,
    string? Currency,
    string? PriceUnit,
    decimal? VenueRating,
    int? VenueReviewCount,
    string? StatusBadge,
    DateTime AddedAt
);

/// <summary>
/// Add to favorites request
/// </summary>
public record AddFavoriteRequest(
    Guid VenueId,
    VenueType VenueType
);
