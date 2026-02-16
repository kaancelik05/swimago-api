using Swimago.Application.DTOs.Common;
using Swimago.Domain.Enums;

namespace Swimago.Application.DTOs.Listings;

public record ListingResponse(
    Guid Id,
    Guid HostId,
    string? Slug,
    ListingType Type,
    string Status,
    bool IsActive,
    bool IsFeatured,
    MultiLanguageDto Title,
    MultiLanguageDto Description,
    MultiLanguageDto Address,
    string? City,
    string? Country,
    decimal Latitude,
    decimal Longitude,
    int MaxGuestCount,
    decimal BasePricePerDay,
    decimal? PriceRangeMin,
    decimal? PriceRangeMax,
    string Currency,
    decimal Rating,
    int ReviewCount,
    IEnumerable<ListingImageDto> Images,
    IEnumerable<AmenityDto>? Amenities = null
);

public record ListingImageDto(
    Guid Id,
    string Url,
    bool IsCover,
    int DisplayOrder,
    string? AltText = null
);

public record AmenityDto(
    Guid Id,
    string Icon,
    MultiLanguageDto Name,
    bool IsActive
);
