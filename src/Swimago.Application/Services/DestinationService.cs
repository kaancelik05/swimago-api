using Microsoft.Extensions.Logging;
using Swimago.Application.DTOs.Destinations;
using Swimago.Application.Interfaces;
using Swimago.Domain.Enums;
using Swimago.Domain.Interfaces;

namespace Swimago.Application.Services;

public class DestinationService : IDestinationService
{
    private readonly IListingRepository _listingRepository;
    private readonly ILogger<DestinationService> _logger;

    public DestinationService(
        IListingRepository listingRepository,
        ILogger<DestinationService> logger)
    {
        _listingRepository = listingRepository;
        _logger = logger;
    }

    public async Task<DestinationListResponse> GetAllDestinationsAsync(
        bool? featured,
        string? type = null,
        string? search = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Fetching destinations: featured={Featured}, type={Type}, search={Search}, page={Page}, pageSize={PageSize}",
            featured,
            type,
            search,
            page,
            pageSize);

        var safePage = Math.Max(1, page);
        var safePageSize = Math.Clamp(pageSize, 1, 100);

        var activeListings = (await _listingRepository.GetActiveListingsAsync(cancellationToken))
            .Where(x => x.Type is ListingType.Beach or ListingType.Pool)
            .ToList();

        var grouped = activeListings
            .GroupBy(x => new { City = x.City.Trim(), Country = x.Country?.Trim() })
            .Select(group =>
            {
                var cityListings = group.ToList();
                var cityType = DetermineDestinationType(cityListings);
                var primaryListing = cityListings
                    .OrderByDescending(x => x.IsFeatured)
                    .ThenByDescending(x => x.Rating)
                    .First();

                return new DestinationItemDto(
                    Id: primaryListing.Id,
                    Slug: FormatSlug(group.Key.City),
                    Name: group.Key.City,
                    Country: group.Key.Country,
                    ImageUrl: primaryListing.Images.FirstOrDefault(x => x.IsCover)?.Url
                        ?? primaryListing.Images.FirstOrDefault()?.Url,
                    SpotCount: cityListings.Count,
                    Type: cityType,
                    MinPrice: cityListings.Min(x => x.BasePricePerDay),
                    MaxPrice: cityListings.Max(x => x.BasePricePerDay),
                    AverageRating: cityListings.Average(x => x.Rating),
                    IsFeatured: cityListings.Any(x => x.IsFeatured)
                );
            });

        if (featured.HasValue)
        {
            grouped = grouped.Where(x => x.IsFeatured == featured.Value);
        }

        if (!string.IsNullOrWhiteSpace(type))
        {
            grouped = grouped.Where(x => x.Type.Equals(type.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchText = search.Trim();
            grouped = grouped.Where(x =>
                x.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                (x.Country != null && x.Country.Contains(searchText, StringComparison.OrdinalIgnoreCase)));
        }

        var ordered = grouped.OrderBy(x => x.Name).ToList();
        var totalCount = ordered.Count;

        var paged = ordered
            .Skip((safePage - 1) * safePageSize)
            .Take(safePageSize)
            .ToList();

        return new DestinationListResponse(paged, totalCount);
    }

    public async Task<DestinationDetailResponse> GetDestinationBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching destination detail: {Slug}", slug);

        var activeListings = (await _listingRepository.GetActiveListingsAsync(cancellationToken))
            .Where(x => x.Type is ListingType.Beach or ListingType.Pool)
            .ToList();

        var cityListings = activeListings
            .Where(x => FormatSlug(x.City).Equals(slug, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (cityListings.Count == 0)
            throw new KeyNotFoundException("Destinasyon bulunamadı");

        var city = cityListings.First().City;
        var country = cityListings.First().Country;

        var spots = cityListings.Select(l => new SpotListItemDto(
            Id: l.Id,
            Slug: l.Slug,
            Name: GetLocalizedText(l.Title),
            VenueType: l.Type == ListingType.Beach ? VenueType.Beach : VenueType.Pool,
            ImageUrl: l.Images.FirstOrDefault(i => i.IsCover)?.Url ?? l.Images.FirstOrDefault()?.Url,
            BasePricePerDay: l.BasePricePerDay,
            Currency: l.PriceCurrency,
            Rating: l.Rating,
            ReviewCount: l.ReviewCount,
            MaxGuestCount: l.MaxGuestCount,
            IsFeatured: l.IsFeatured,
            Amenities: l.Amenities
                .Select(x => GetLocalizedText(x.Amenity?.Label))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Take(5)
                .ToList()
        )).ToList();

        return new DestinationDetailResponse(
            Id: cityListings.First().Id,
            Slug: slug,
            Name: city,
            Description: $"{city} destinasyonundaki beach ve pool mekanlarını keşfedin.",
            Country: country,
            ImageUrl: cityListings
                .OrderByDescending(x => x.IsFeatured)
                .SelectMany(x => x.Images)
                .OrderBy(x => x.DisplayOrder)
                .FirstOrDefault(x => x.IsCover)?.Url
                    ?? cityListings.SelectMany(x => x.Images).OrderBy(x => x.DisplayOrder).FirstOrDefault()?.Url,
            Latitude: cityListings.Average(x => x.Latitude),
            Longitude: cityListings.Average(x => x.Longitude),
            Spots: spots,
            Tags: cityListings
                .SelectMany(x => x.Amenities)
                .Select(x => GetLocalizedText(x.Amenity?.Label))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(10)
                .ToList(),
            AverageRating: cityListings.Any() ? cityListings.Average(x => x.Rating) : null,
            ReviewCount: cityListings.Sum(x => x.ReviewCount)
        );
    }

    public async Task<DestinationPageDetailResponse> GetDestinationPageBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var basic = await GetDestinationBySlugAsync(slug, cancellationToken);

        var type = DetermineDestinationTypeFromSpots(basic.Spots);

        var features = new List<DestinationFeatureItemDto>
        {
            new("wb_sunny", "Perfect Weather", "Güneşli günler ve temiz hava ile keyifli bir deneyim."),
            new("map", "Easy Access", "Şehir merkezine ve popüler noktalara hızlı ulaşım."),
            new("local_activity", "Premium Spots", "Yüksek puanlı beach ve pool mekanları bir arada.")
        };

        var spots = basic.Spots.Select(x => new DestinationSpotItemDto(
            Id: x.Id,
            Slug: x.Slug,
            Name: x.Name,
            Location: basic.Name,
            ImageUrl: x.ImageUrl,
            Rating: x.Rating,
            ReviewCount: x.ReviewCount,
            Price: x.BasePricePerDay,
            Currency: x.Currency,
            PriceUnit: "day"
        )).ToList();

        return new DestinationPageDetailResponse(
            Id: basic.Id,
            Slug: basic.Slug,
            Type: type,
            Hero: new DestinationHeroDto(
                Title: basic.Name,
                Subtitle: $"{basic.Name} için seçkin {type.ToLowerInvariant()} deneyimleri",
                Location: string.IsNullOrWhiteSpace(basic.Country) ? basic.Name : $"{basic.Name}, {basic.Country}",
                ImageUrl: basic.ImageUrl,
                SpotCount: spots.Count
            ),
            Overview: new DestinationOverviewDto(
                Description: basic.Description,
                AvgWaterTemp: null,
                SunnyDaysPerYear: null,
                MapImageUrl: null
            ),
            Features: features,
            Spots: spots,
            Cta: new DestinationCtaDto(
                Title: "Aradığını bulamadın mı?",
                Description: "Yakındaki diğer popüler destinasyonları da inceleyebilirsin.",
                ButtonText: type.Equals("Beach", StringComparison.OrdinalIgnoreCase)
                    ? "Yakındaki Beach'leri Keşfet"
                    : "Yakındaki Pool'ları Keşfet",
                BackgroundImageUrl: basic.ImageUrl
            )
        );
    }

    private static string DetermineDestinationType(IEnumerable<Domain.Entities.Listing> listings)
    {
        var beachCount = listings.Count(x => x.Type == ListingType.Beach);
        var poolCount = listings.Count(x => x.Type == ListingType.Pool);
        return beachCount >= poolCount ? "Beach" : "Pool";
    }

    private static string DetermineDestinationTypeFromSpots(IEnumerable<SpotListItemDto> spots)
    {
        var beachCount = spots.Count(x => x.VenueType == VenueType.Beach);
        var poolCount = spots.Count(x => x.VenueType == VenueType.Pool);
        return beachCount >= poolCount ? "Beach" : "Pool";
    }

    private static string FormatSlug(string text)
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

    private static string GetLocalizedText(Dictionary<string, string>? values)
    {
        if (values == null || values.Count == 0)
        {
            return string.Empty;
        }

        if (values.TryGetValue("tr", out var tr) && !string.IsNullOrWhiteSpace(tr))
        {
            return tr;
        }

        if (values.TryGetValue("en", out var en) && !string.IsNullOrWhiteSpace(en))
        {
            return en;
        }

        return values.Values.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)) ?? string.Empty;
    }
}
