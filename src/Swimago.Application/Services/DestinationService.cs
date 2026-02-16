using Microsoft.Extensions.Logging;
using Swimago.Application.DTOs.Destinations;
using Swimago.Application.Interfaces;
using Swimago.Domain.Entities;
using Swimago.Domain.Enums;
using Swimago.Domain.Interfaces;

namespace Swimago.Application.Services;

public class DestinationService : IDestinationService
{
    private readonly ICityRepository _cityRepository;
    private readonly IListingRepository _listingRepository;
    private readonly ILogger<DestinationService> _logger;

    public DestinationService(
        ICityRepository cityRepository,
        IListingRepository listingRepository,
        ILogger<DestinationService> logger)
    {
        _cityRepository = cityRepository;
        _listingRepository = listingRepository;
        _logger = logger;
    }

    public async Task<DestinationListResponse> GetAllDestinationsAsync(bool? featured, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching destinations, featured: {Featured}", featured);

        var cities = await _cityRepository.GetAllAsync(cancellationToken);
        var activeListings = await _listingRepository.GetActiveListingsAsync(cancellationToken);

        var destinations = cities
            .Where(c => c.IsActive)
            .Select(city =>
            {
                var cityName = city.Name.GetValueOrDefault("tr") ?? city.Name.Values.FirstOrDefault() ?? "";
                var cityListings = activeListings.Where(l => 
                    l.City.Equals(cityName, StringComparison.OrdinalIgnoreCase) ||
                    l.City.Equals(city.Name.GetValueOrDefault("en") ?? "", StringComparison.OrdinalIgnoreCase));
                
                return new DestinationItemDto(
                    Id: city.Id,
                    Slug: FormatSlug(cityName),
                    Name: cityName,
                    Country: city.Country,
                    ImageUrl: null, 
                    SpotCount: cityListings.Count(),
                    MinPrice: cityListings.Any() ? cityListings.Min(l => l.BasePricePerDay) : null,
                    MaxPrice: cityListings.Any() ? cityListings.Max(l => l.BasePricePerDay) : null,
                    AverageRating: cityListings.Any() ? cityListings.Average(l => l.Rating) : null,
                    IsFeatured: false 
                );
            })
            .Where(d => d.SpotCount > 0);

        var destinationList = destinations.OrderBy(d => d.Name).ToList();

        return new DestinationListResponse(destinationList, destinationList.Count);
    }

    public async Task<DestinationDetailResponse> GetDestinationBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching destination: {Slug}", slug);

        var cities = await _cityRepository.GetAllAsync(cancellationToken);
        var city = cities.FirstOrDefault(c => 
        {
            var cityName = c.Name.GetValueOrDefault("tr") ?? c.Name.Values.FirstOrDefault() ?? "";
            var citySlug = FormatSlug(cityName);
            return citySlug.Equals(slug, StringComparison.OrdinalIgnoreCase);
        });

        if (city == null)
            throw new KeyNotFoundException("Destinasyon bulunamadı");

        var cityName = city.Name.GetValueOrDefault("tr") ?? city.Name.Values.FirstOrDefault() ?? "";
        var activeListings = await _listingRepository.GetActiveListingsAsync(cancellationToken);
        var cityListings = activeListings.Where(l => 
            l.City.Equals(cityName, StringComparison.OrdinalIgnoreCase) ||
            l.City.Equals(city.Name.GetValueOrDefault("en") ?? "", StringComparison.OrdinalIgnoreCase)).ToList();

        var spots = cityListings.Select(l => new SpotListItemDto(
            Id: l.Id,
            Slug: l.Slug,
            Name: l.Title.GetValueOrDefault("tr") ?? "",
            VenueType: l.Type switch
            {
                ListingType.Beach => VenueType.Beach,
                ListingType.Pool => VenueType.Pool,
                ListingType.Yacht => VenueType.Yacht,
                ListingType.DayTrip => VenueType.DayTrip,
                _ => VenueType.Beach
            },
            ImageUrl: l.Images.FirstOrDefault(i => i.IsCover)?.Url ?? l.Images.FirstOrDefault()?.Url,
            BasePricePerDay: l.BasePricePerDay,
            Currency: l.PriceCurrency,
            Rating: l.Rating,
            ReviewCount: l.ReviewCount,
            MaxGuestCount: l.MaxGuestCount,
            IsFeatured: l.IsFeatured,
            Amenities: null // Or fetch basic amenities
        )).ToList();

        return new DestinationDetailResponse(
            Id: city.Id,
            Slug: slug,
            Name: cityName,
            Description: "Harika plajlar ve eğlenceli mekanlar.", // Placeholder
            Country: city.Country,
            ImageUrl: null,
            Latitude: null, // City entity might need lat/long or get from first listing? Sticking to null/optional
            Longitude: null,
            Spots: spots,
            Tags: null,
            AverageRating: cityListings.Any() ? cityListings.Average(l => l.Rating) : null,
            ReviewCount: cityListings.Sum(l => l.ReviewCount)
        );
    }

    private string FormatSlug(string text)
    {
        return text.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("ı", "i")
            .Replace("ğ", "g")
            .Replace("ü", "u")
            .Replace("ş", "s")
            .Replace("ö", "o")
            .Replace("ç", "c");
    }
}
