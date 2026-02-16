using Microsoft.Extensions.Logging;
using Swimago.Application.DTOs.BoatTours;
using Swimago.Application.DTOs.Common;
using Swimago.Application.DTOs.Destinations;
using Swimago.Application.Interfaces;
using Swimago.Domain.Entities;
using Swimago.Domain.Enums;
using Swimago.Domain.Interfaces;
using System.Text.RegularExpressions;

namespace Swimago.Application.Services;

public class BoatTourService : IBoatTourService
{
    private readonly IListingRepository _listingRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<BoatTourService> _logger;

    public BoatTourService(
        IListingRepository listingRepository,
        IUserRepository userRepository,
        ILogger<BoatTourService> logger)
    {
        _listingRepository = listingRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<BoatTourListResponse> GetAllBoatToursAsync(string? city, string? type, decimal? minPrice, decimal? maxPrice, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching boat tours: city={City}, type={Type}", city, type);

        var activeListings = await _listingRepository.GetActiveListingsAsync(cancellationToken);

        var boatListings = activeListings.Where(l => 
            l.Type == ListingType.Yacht || l.Type == ListingType.DayTrip);

        if (!string.IsNullOrEmpty(city))
            boatListings = boatListings.Where(l => l.City.Contains(city, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(type))
        {
            if (type.Equals("yacht", StringComparison.OrdinalIgnoreCase))
                boatListings = boatListings.Where(l => l.Type == ListingType.Yacht);
            else if (type.Equals("day-trip", StringComparison.OrdinalIgnoreCase))
                boatListings = boatListings.Where(l => l.Type == ListingType.DayTrip);
        }

        if (minPrice.HasValue)
            boatListings = boatListings.Where(l => l.BasePricePerDay >= minPrice.Value);

        if (maxPrice.HasValue)
            boatListings = boatListings.Where(l => l.BasePricePerDay <= maxPrice.Value);

        var listingsList = boatListings.ToList();

        var yachtTours = listingsList
            .Where(l => l.Type == ListingType.Yacht)
            .Select(l => new YachtTourItemDto(
                Id: l.Id,
                Slug: l.Slug,
                Name: l.Title.GetValueOrDefault("tr") ?? "",
                ImageUrl: l.Images.FirstOrDefault(i => i.IsCover)?.Url ?? l.Images.FirstOrDefault()?.Url,
                PricePerDay: l.BasePricePerDay,
                Currency: l.PriceCurrency,
                Capacity: l.MaxGuestCount,
                Length: null,
                CabinCount: null,
                Rating: l.Rating,
                ReviewCount: l.ReviewCount,
                City: l.City,
                IsFeatured: l.IsFeatured,
                Highlights: l.Amenities.Take(3).Select(a => a.Amenity?.Label.GetValueOrDefault("tr") ?? "").Where(n => !string.IsNullOrEmpty(n))
            )).ToList();

        var dayTrips = listingsList
            .Where(l => l.Type == ListingType.DayTrip)
            .Select(l => new DayTripItemDto(
                Id: l.Id,
                Slug: l.Slug,
                Name: l.Title.GetValueOrDefault("tr") ?? "",
                ImageUrl: l.Images.FirstOrDefault(i => i.IsCover)?.Url ?? l.Images.FirstOrDefault()?.Url,
                PricePerPerson: l.BasePricePerDay,
                Currency: l.PriceCurrency,
                MinCapacity: 1,
                MaxCapacity: l.MaxGuestCount,
                Duration: ParseDuration(l.Duration),
                Rating: l.Rating,
                ReviewCount: l.ReviewCount,
                City: l.City,
                DeparturePoint: null,
                IsFeatured: l.IsFeatured,
                Highlights: l.Amenities.Take(3).Select(a => a.Amenity?.Label.GetValueOrDefault("tr") ?? "").Where(n => !string.IsNullOrEmpty(n))
            )).ToList();

        return new BoatTourListResponse(yachtTours, dayTrips, listingsList.Count);
    }

    public async Task<YachtTourDetailResponse> GetYachtTourBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching yacht tour: {Slug}", slug);

        var listing = await _listingRepository.GetBySlugAsync(slug, cancellationToken);
        
        if (listing == null || listing.Type != ListingType.Yacht)
            throw new KeyNotFoundException("Yat turu bulunamadı");

        var host = await _userRepository.GetByIdAsync(listing.HostId, cancellationToken);
        var hostListings = host != null 
            ? await _listingRepository.GetByHostIdAsync(listing.HostId, cancellationToken)
            : Enumerable.Empty<Listing>();

        return new YachtTourDetailResponse(
            Id: listing.Id,
            Slug: listing.Slug,
            Name: MultiLanguageDto.FromDictionary(listing.Title),
            Description: MultiLanguageDto.FromDictionary(listing.Description),
            Status: listing.Status.ToString(),
            City: listing.City,
            Country: listing.Country,
            Latitude: listing.Latitude,
            Longitude: listing.Longitude,
            PricePerDay: listing.BasePricePerDay,
            PricePerWeek: null,
            Currency: listing.PriceCurrency,
            Capacity: listing.MaxGuestCount,
            Length: null,
            CabinCount: null,
            BathroomCount: null,
            CrewCount: null,
            YearBuilt: null,
            Manufacturer: null,
            Model: null,
            Rating: listing.Rating,
            ReviewCount: listing.ReviewCount,
            IsFeatured: listing.IsFeatured,
            Images: listing.Images.OrderBy(i => i.DisplayOrder).Select(i => new SpotImageDto(
                Id: i.Id,
                Url: i.Url,
                IsCover: i.IsCover,
                DisplayOrder: i.DisplayOrder,
                AltText: i.Alt
            )),
            Amenities: listing.Amenities.Select(la => new SpotAmenityDto(
                Id: la.Amenity?.Id ?? Guid.Empty,
                Name: la.Amenity?.Label.GetValueOrDefault("tr") ?? "",
                Icon: la.Amenity?.Icon,
                Category: la.Amenity?.Category
            )),
            Conditions: null,
            IncludedServices: null,
            ExcludedServices: null,
            Host: new HostInfoDto(
                Id: listing.HostId,
                Name: host?.Profile != null 
                    ? $"{host.Profile.FirstName.GetValueOrDefault("tr")} {host.Profile.LastName.GetValueOrDefault("tr")}".Trim()
                    : host?.Email ?? "Host",
                Avatar: host?.Profile?.Avatar,
                MemberSince: host?.CreatedAt ?? DateTime.UtcNow,
                ListingCount: hostListings.Count(),
                AverageRating: hostListings.Any() ? hostListings.Average(l => l.Rating) : 0,
                ResponseRate: 95,
                ResponseTime: "1 saat içinde"
            ),
            AvailableSlots: null
        );
    }

    public async Task<DayTripDetailResponse> GetDayTripBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching day trip: {Slug}", slug);

        var listing = await _listingRepository.GetBySlugAsync(slug, cancellationToken);
        
        if (listing == null || listing.Type != ListingType.DayTrip)
            throw new KeyNotFoundException("Günlük tur bulunamadı");

        var host = await _userRepository.GetByIdAsync(listing.HostId, cancellationToken);
        var hostListings = host != null 
            ? await _listingRepository.GetByHostIdAsync(listing.HostId, cancellationToken)
            : Enumerable.Empty<Listing>();

        return new DayTripDetailResponse(
            Id: listing.Id,
            Slug: listing.Slug,
            Name: MultiLanguageDto.FromDictionary(listing.Title),
            Description: MultiLanguageDto.FromDictionary(listing.Description),
            Status: listing.Status.ToString(),
            City: listing.City,
            Country: listing.Country,
            Latitude: listing.Latitude,
            Longitude: listing.Longitude,
            PricePerPerson: listing.BasePricePerDay,
            Currency: listing.PriceCurrency,
            MinCapacity: 1,
            MaxCapacity: listing.MaxGuestCount,
            Duration: ParseDuration(listing.Duration),
            DepartureTime: null,
            DeparturePoint: null,
            Route: null,
            Rating: listing.Rating,
            ReviewCount: listing.ReviewCount,
            IsFeatured: listing.IsFeatured,
            Images: listing.Images.OrderBy(i => i.DisplayOrder).Select(i => new SpotImageDto(
                Id: i.Id,
                Url: i.Url,
                IsCover: i.IsCover,
                DisplayOrder: i.DisplayOrder,
                AltText: i.Alt
            )),
            Amenities: listing.Amenities.Select(la => new SpotAmenityDto(
                Id: la.Amenity?.Id ?? Guid.Empty,
                Name: la.Amenity?.Label.GetValueOrDefault("tr") ?? "",
                Icon: la.Amenity?.Icon,
                Category: la.Amenity?.Category
            )),
            Conditions: null,
            IncludedServices: null,
            ExcludedServices: null,
            Host: new HostInfoDto(
                Id: listing.HostId,
                Name: host?.Profile != null 
                    ? $"{host.Profile.FirstName.GetValueOrDefault("tr")} {host.Profile.LastName.GetValueOrDefault("tr")}".Trim()
                    : host?.Email ?? "Host",
                Avatar: host?.Profile?.Avatar,
                MemberSince: host?.CreatedAt ?? DateTime.UtcNow,
                ListingCount: hostListings.Count(),
                AverageRating: hostListings.Any() ? hostListings.Average(l => l.Rating) : 0,
                ResponseRate: 95,
                ResponseTime: "1 saat içinde"
            ),
            AvailableSlots: null
        );
    }

    private TimeSpan ParseDuration(string? duration)
    {
        if (string.IsNullOrEmpty(duration))
            return TimeSpan.Zero;

        // Try parsing "X hours"
        var hoursMatch = Regex.Match(duration, @"(\d+)\s*hour", RegexOptions.IgnoreCase);
        if (hoursMatch.Success && int.TryParse(hoursMatch.Groups[1].Value, out int hours))
        {
            return TimeSpan.FromHours(hours);
        }

        if (duration.Contains("Full day", StringComparison.OrdinalIgnoreCase) || 
            duration.Contains("Gün boyu", StringComparison.OrdinalIgnoreCase))
        {
            return TimeSpan.FromHours(8); // Standard full day
        }
        
        if (duration.Contains("Half day", StringComparison.OrdinalIgnoreCase) || 
            duration.Contains("Yarım gün", StringComparison.OrdinalIgnoreCase))
        {
            return TimeSpan.FromHours(4); // Standard half day
        }

        return TimeSpan.Zero;
    }
}
