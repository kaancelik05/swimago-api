using Swimago.Application.DTOs.Common;
using Swimago.Application.DTOs.Destinations;
using Swimago.Domain.Enums;

namespace Swimago.Application.DTOs.BoatTours;

/// <summary>
/// Boat tours list response (yacht + day trips)
/// </summary>
public record BoatTourListResponse(
    IEnumerable<YachtTourItemDto> YachtTours,
    IEnumerable<DayTripItemDto> DayTrips,
    int TotalCount
);

/// <summary>
/// Yacht tour list item
/// </summary>
public record YachtTourItemDto(
    Guid Id,
    string Slug,
    string Name,
    string? ImageUrl,
    decimal PricePerDay,
    string Currency,
    int Capacity,
    decimal? Length,
    int? CabinCount,
    decimal Rating,
    int ReviewCount,
    string? City,
    bool IsFeatured,
    IEnumerable<string>? Highlights
);

/// <summary>
/// Day trip list item
/// </summary>
public record DayTripItemDto(
    Guid Id,
    string Slug,
    string Name,
    string? ImageUrl,
    decimal PricePerPerson,
    string Currency,
    int MinCapacity,
    int MaxCapacity,
    TimeSpan Duration,
    decimal Rating,
    int ReviewCount,
    string? City,
    string? DeparturePoint,
    bool IsFeatured,
    IEnumerable<string>? Highlights
);

/// <summary>
/// Yacht tour detail response
/// </summary>
public record YachtTourDetailResponse(
    Guid Id,
    string Slug,
    MultiLanguageDto Name,
    MultiLanguageDto? Description,
    string Status,
    string? City,
    string? Country,
    decimal Latitude,
    decimal Longitude,
    decimal PricePerDay,
    decimal? PricePerWeek,
    string Currency,
    int Capacity,
    decimal? Length,
    int? CabinCount,
    int? BathroomCount,
    int? CrewCount,
    int? YearBuilt,
    string? Manufacturer,
    string? Model,
    decimal Rating,
    int ReviewCount,
    bool IsFeatured,
    IEnumerable<SpotImageDto> Images,
    IEnumerable<SpotAmenityDto> Amenities,
    MultiLanguageDto? Conditions,
    MultiLanguageDto? IncludedServices,
    MultiLanguageDto? ExcludedServices,
    HostInfoDto Host,
    IEnumerable<AvailabilitySlotDto>? AvailableSlots
);

/// <summary>
/// Day trip detail response
/// </summary>
public record DayTripDetailResponse(
    Guid Id,
    string Slug,
    MultiLanguageDto Name,
    MultiLanguageDto? Description,
    string Status,
    string? City,
    string? Country,
    decimal Latitude,
    decimal Longitude,
    decimal PricePerPerson,
    string Currency,
    int MinCapacity,
    int MaxCapacity,
    TimeSpan Duration,
    TimeSpan? DepartureTime,
    string? DeparturePoint,
    MultiLanguageDto? Route,
    decimal Rating,
    int ReviewCount,
    bool IsFeatured,
    IEnumerable<SpotImageDto> Images,
    IEnumerable<SpotAmenityDto> Amenities,
    MultiLanguageDto? Conditions,
    MultiLanguageDto? IncludedServices,
    MultiLanguageDto? ExcludedServices,
    HostInfoDto Host,
    IEnumerable<AvailabilitySlotDto>? AvailableSlots
);
