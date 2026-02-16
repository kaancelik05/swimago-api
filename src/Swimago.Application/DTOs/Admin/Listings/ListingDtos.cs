using System.ComponentModel.DataAnnotations;
using Swimago.Application.DTOs.Admin.Shared;
using Swimago.Domain.Enums;

namespace Swimago.Application.DTOs.Admin.Listings;

// --- SHARED LISTING DTO ---
public class ListingListItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public double Rating { get; set; }
    public int ReviewCount { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public ListingType Type { get; set; }
}

// --- BEACH DTOs ---
public class BeachDetailDto
{
    public Guid Id { get; set; }
    public MultiLanguageDto Name { get; set; } = new();
    public string Slug { get; set; } = string.Empty;
    public MultiLanguageDto Description { get; set; } = new();
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? LocationSubtitle { get; set; }
    public string? MapImageUrl { get; set; }
    public decimal PricePerDay { get; set; }
    public string Currency { get; set; } = "USD";
    public string PriceUnit { get; set; } = "day";
    public List<ImageDto> Images { get; set; } = new();
    public BeachConditionsDto Conditions { get; set; } = new();
    public List<AmenityDto> Amenities { get; set; } = new();
    public string? RareFindMessage { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public List<BreadcrumbItemDto> Breadcrumbs { get; set; } = new();
}

public class BeachConditionsDto
{
    public string WindSpeed { get; set; } = string.Empty;
    public string WaterDepth { get; set; } = string.Empty;
    public string GroundType { get; set; } = string.Empty;
    public string WaveStatus { get; set; } = string.Empty;
}

public class CreateBeachRequest
{
    [Required] public MultiLanguageDto Name { get; set; } = new();
    [Required] public string Slug { get; set; } = string.Empty;
    [Required] public MultiLanguageDto Description { get; set; } = new();
    [Required] public string City { get; set; } = string.Empty;
    [Required] public string Country { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? LocationSubtitle { get; set; }
    public string? MapImageUrl { get; set; }
    [Required] public decimal PricePerDay { get; set; }
    public string Currency { get; set; } = "USD";
    public string PriceUnit { get; set; } = "day";
    public List<ImageDto> Images { get; set; } = new();
    public BeachConditionsDto Conditions { get; set; } = new();
    public List<AmenityDto> Amenities { get; set; } = new();
    public string? RareFindMessage { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; }
    public List<BreadcrumbItemDto> Breadcrumbs { get; set; } = new();
}

// --- POOL DTOs ---
public class PoolDetailDto
{
    public Guid Id { get; set; }
    public MultiLanguageDto Name { get; set; } = new();
    public string Slug { get; set; } = string.Empty;
    public MultiLanguageDto Description { get; set; } = new();
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? LocationSubtitle { get; set; }
    public string? MapImageUrl { get; set; }
    public decimal PricePerDay { get; set; }
    public string Currency { get; set; } = "USD";
    public string PriceUnit { get; set; } = "day";
    public List<ImageDto> Images { get; set; } = new();
    public PoolConditionsDto Conditions { get; set; } = new();
    public List<AmenityDto> Amenities { get; set; } = new();
    public string? RareFindMessage { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public List<BreadcrumbItemDto> Breadcrumbs { get; set; } = new();
}

public class PoolConditionsDto
{
    public string WaterTemperature { get; set; } = string.Empty;
    public string PoolDepth { get; set; } = string.Empty;
    public string PoolLength { get; set; } = string.Empty;
    public string SwimmingLanes { get; set; } = string.Empty;
}

public class CreatePoolRequest
{
    [Required] public MultiLanguageDto Name { get; set; } = new();
    [Required] public string Slug { get; set; } = string.Empty;
    [Required] public MultiLanguageDto Description { get; set; } = new();
    [Required] public string City { get; set; } = string.Empty;
    [Required] public string Country { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? LocationSubtitle { get; set; }
    public string? MapImageUrl { get; set; }
    [Required] public decimal PricePerDay { get; set; }
    public string Currency { get; set; } = "USD";
    public string PriceUnit { get; set; } = "day";
    public List<ImageDto> Images { get; set; } = new();
    public PoolConditionsDto Conditions { get; set; } = new();
    public List<AmenityDto> Amenities { get; set; } = new();
    public string? RareFindMessage { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; }
    public List<BreadcrumbItemDto> Breadcrumbs { get; set; } = new();
}

// --- YACHT TOUR DTOs ---
public class YachtTourDetailDto
{
    public Guid Id { get; set; }
    public MultiLanguageDto Name { get; set; } = new();
    public string Slug { get; set; } = string.Empty;
    public MultiLanguageDto Description { get; set; } = new();
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public decimal PricePerDay { get; set; }
    public string Currency { get; set; } = "USD";
    public int MinCapacity { get; set; }
    public int MaxCapacity { get; set; }
    public string BoatType { get; set; } = string.Empty;
    public double? BoatLength { get; set; }
    public int? CabinCount { get; set; }
    public int? BathroomCount { get; set; }
    public int? ManufacturerYear { get; set; }
    public List<ImageDto> Images { get; set; } = new();
    public string? RouteMapImage { get; set; }
    public List<YachtSpecDto> Specs { get; set; } = new();
    public List<YachtFeatureDto> Features { get; set; } = new();
    public List<AccommodationOptionDto> AccommodationOptions { get; set; } = new();
    public List<CateringItemDto> CateringItems { get; set; } = new();
    public List<ActivityItemDto> ActivityItems { get; set; } = new();
    public RouteInfoDto CruisingRoute { get; set; } = new();
    public MultiLanguageDto? Conditions { get; set; }
    public MultiLanguageDto? IncludedServices { get; set; }
    public MultiLanguageDto? ExcludedServices { get; set; }
    public List<BookingBreakdownItemDto> Breakdown { get; set; } = new();
    public string? LuxuryPromise { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsSuperhost { get; set; }
}

public class YachtSpecDto
{
    public string Icon { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public class YachtFeatureDto
{
    public string Icon { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}

public class AccommodationOptionDto
{
    public string ImageUrl { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class CateringItemDto
{
    public string Icon { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}

public class ActivityItemDto
{
    public string Icon { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}

public class RouteInfoDto
{
    public string Name { get; set; } = string.Empty;
    public string Stops { get; set; } = string.Empty;
    public int TotalRoutes { get; set; }
}

public class CreateYachtTourRequest
{
    [Required] public MultiLanguageDto Name { get; set; } = new();
    [Required] public string Slug { get; set; } = string.Empty;
    [Required] public MultiLanguageDto Description { get; set; } = new();
    [Required] public string City { get; set; } = string.Empty;
    [Required] public string Country { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    [Required] public decimal PricePerDay { get; set; }
    public string Currency { get; set; } = "USD";
    public int MinCapacity { get; set; } = 1;
    [Required] public int MaxCapacity { get; set; }
    [Required] public string BoatType { get; set; } = string.Empty;
    public double? BoatLength { get; set; }
    public int? CabinCount { get; set; }
    public int? BathroomCount { get; set; }
    public int? ManufacturerYear { get; set; }
    public List<ImageDto> Images { get; set; } = new();
    public string? RouteMapImage { get; set; }
    public List<YachtSpecDto> Specs { get; set; } = new();
    public List<YachtFeatureDto> Features { get; set; } = new();
    public List<AccommodationOptionDto> AccommodationOptions { get; set; } = new();
    public List<CateringItemDto> CateringItems { get; set; } = new();
    public List<ActivityItemDto> ActivityItems { get; set; } = new();
    public RouteInfoDto CruisingRoute { get; set; } = new();
    public MultiLanguageDto? Conditions { get; set; }
    public MultiLanguageDto? IncludedServices { get; set; }
    public MultiLanguageDto? ExcludedServices { get; set; }
    public List<BookingBreakdownItemDto> Breakdown { get; set; } = new();
    public string? LuxuryPromise { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsSuperhost { get; set; }
}

// --- DAY TRIP DTOs ---
public class DayTripDetailDto
{
    public Guid Id { get; set; }
    public MultiLanguageDto Name { get; set; } = new();
    public string Slug { get; set; } = string.Empty;
    public List<string> Description { get; set; } = new();
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? DeparturePoint { get; set; }
    public decimal PricePerPerson { get; set; }
    public decimal? PrivateCharterPrice { get; set; }
    public string Currency { get; set; } = "USD";
    public int? MinCapacity { get; set; }
    public int MaxCapacity { get; set; }
    public string Duration { get; set; } = string.Empty;
    public string? DepartureTime { get; set; }
    public List<ImageDto> Images { get; set; } = new();
    public HostInfoDto Host { get; set; } = new();
    public List<TourInfoBadgeDto> InfoBadges { get; set; } = new();
    public List<RouteStopDto> RouteStops { get; set; } = new();
    public List<AmenityDto> Amenities { get; set; } = new();
    public List<FoodItemDto> FoodItems { get; set; } = new();
    public List<ActivityTagDto> ActivityTags { get; set; } = new();
    public MusicInfoDto? MusicInfo { get; set; }
    public MultiLanguageDto? Route { get; set; }
    public MultiLanguageDto? Conditions { get; set; }
    public MultiLanguageDto? IncludedServices { get; set; }
    public MultiLanguageDto? ExcludedServices { get; set; }
    public bool? IsPrivateCharter { get; set; }
    public List<BookingBreakdownItemDto> Breakdown { get; set; } = new();
    public string? RareFindMessage { get; set; }
    public string? RareFindSubtitle { get; set; }
    public bool IsFeatured { get; set; }
    public bool? IsSuperhost { get; set; }
}

public class HostInfoDto
{
    public string AvatarUrl { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Experience { get; set; }
    public int? YearStarted { get; set; }
}

public class TourInfoBadgeDto
{
    public string Icon { get; set; } = string.Empty;
    public string? Label { get; set; }
    public string? Value { get; set; }
    public string? Text { get; set; }
}

public class RouteStopDto
{
    public string Time { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class FoodItemDto
{
    public string Icon { get; set; } = string.Empty;
    public string? IconColor { get; set; }
    public string? Text { get; set; }
    public string? Name { get; set; }
}

public class ActivityTagDto
{
    public string Label { get; set; } = string.Empty;
    public string? Icon { get; set; }
}

public class MusicInfoDto
{
    public string Text { get; set; } = string.Empty;
}

public class CreateDayTripRequest
{
    [Required] public MultiLanguageDto Name { get; set; } = new();
    [Required] public string Slug { get; set; } = string.Empty;
    public List<string> Description { get; set; } = new();
    [Required] public string City { get; set; } = string.Empty;
    [Required] public string Country { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? DeparturePoint { get; set; }
    [Required] public decimal PricePerPerson { get; set; }
    public decimal? PrivateCharterPrice { get; set; }
    public string Currency { get; set; } = "USD";
    public int? MinCapacity { get; set; }
    [Required] public int MaxCapacity { get; set; }
    [Required] public string Duration { get; set; } = string.Empty;
    public string? DepartureTime { get; set; }
    public List<ImageDto> Images { get; set; } = new();
    [Required] public HostInfoDto Host { get; set; } = new();
    public List<TourInfoBadgeDto> InfoBadges { get; set; } = new();
    public List<RouteStopDto> RouteStops { get; set; } = new();
    public List<AmenityDto> Amenities { get; set; } = new();
    public List<FoodItemDto> FoodItems { get; set; } = new();
    public List<ActivityTagDto> ActivityTags { get; set; } = new();
    public MusicInfoDto? MusicInfo { get; set; }
    public MultiLanguageDto? Route { get; set; }
    public MultiLanguageDto? Conditions { get; set; }
    public MultiLanguageDto? IncludedServices { get; set; }
    public MultiLanguageDto? ExcludedServices { get; set; }
    public bool? IsPrivateCharter { get; set; }
    public List<BookingBreakdownItemDto> Breakdown { get; set; } = new();
    public string? RareFindMessage { get; set; }
    public string? RareFindSubtitle { get; set; }
    public bool IsFeatured { get; set; }
    public bool? IsSuperhost { get; set; }
}
