using AutoMapper;
using Swimago.Application.DTOs.Common;
using Swimago.Application.DTOs.Listings;
using Swimago.Application.DTOs.Search;
using Swimago.Application.Interfaces;
using Swimago.Domain.Entities;
using Swimago.Domain.Enums;
using Swimago.Domain.Interfaces;

namespace Swimago.Application.Services;

public class SearchService : ISearchService
{
    private readonly IListingRepository _listingRepository;
    private readonly IFavoriteRepository _favoriteRepository;
    private readonly IMapper _mapper;

    public SearchService(
        IListingRepository listingRepository,
        IFavoriteRepository favoriteRepository,
        IMapper mapper)
    {
        _listingRepository = listingRepository;
        _favoriteRepository = favoriteRepository;
        _mapper = mapper;
    }

    public async Task<SearchListingsResponse> SearchListingsAsync(SearchListingsQuery query, CancellationToken cancellationToken = default)
    {
        var allListings = await _listingRepository.GetActiveListingsAsync(cancellationToken);
        var filteredListings = allListings.AsQueryable();

        if (query.Type.HasValue)
        {
            filteredListings = filteredListings.Where(l => l.Type == query.Type.Value);
        }

        if (query.MinGuestCount.HasValue)
        {
            filteredListings = filteredListings.Where(l => l.MaxGuestCount >= query.MinGuestCount.Value);
        }

        if (query.MinPrice.HasValue)
        {
            filteredListings = filteredListings.Where(l => l.BasePricePerDay >= query.MinPrice.Value);
        }

        if (query.MaxPrice.HasValue)
        {
            filteredListings = filteredListings.Where(l => l.BasePricePerDay <= query.MaxPrice.Value);
        }

        if (query.MinRating.HasValue)
        {
            filteredListings = filteredListings.Where(l => l.Rating >= query.MinRating.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.City))
        {
            filteredListings = filteredListings.Where(l => l.City.ToLower().Contains(query.City.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var searchTermLower = query.SearchTerm.ToLower();
            filteredListings = filteredListings.Where(l =>
                l.Title.Values.Any(v => v.ToLower().Contains(searchTermLower)) ||
                l.Description.Values.Any(v => v.ToLower().Contains(searchTermLower)) ||
                l.Address.Values.Any(v => v.ToLower().Contains(searchTermLower)) ||
                l.City.ToLower().Contains(searchTermLower)
            );
        }

        if (query.AmenityIds != null && query.AmenityIds.Any())
        {
            filteredListings = filteredListings.Where(l =>
                query.AmenityIds.All(amenityId =>
                    l.Amenities.Any(la => la.AmenityId == amenityId)
                )
            );
        }

        if (query.Latitude.HasValue && query.Longitude.HasValue && query.RadiusKm.HasValue)
        {
            filteredListings = filteredListings.Where(l =>
                CalculateDistance(query.Latitude.Value, query.Longitude.Value, l.Latitude, l.Longitude) <= (double)query.RadiusKm.Value
            );
        }

        var totalCount = filteredListings.Count();

        filteredListings = query.SortBy switch
        {
            SearchSortBy.Price => query.SortDescending
                ? filteredListings.OrderByDescending(l => l.BasePricePerDay)
                : filteredListings.OrderBy(l => l.BasePricePerDay),

            SearchSortBy.Rating => query.SortDescending
                ? filteredListings.OrderByDescending(l => l.Rating)
                : filteredListings.OrderBy(l => l.Rating),

            SearchSortBy.ReviewCount => query.SortDescending
                ? filteredListings.OrderByDescending(l => l.ReviewCount)
                : filteredListings.OrderBy(l => l.ReviewCount),

            SearchSortBy.CreatedDate => query.SortDescending
                ? filteredListings.OrderByDescending(l => l.CreatedAt)
                : filteredListings.OrderBy(l => l.CreatedAt),

            SearchSortBy.Distance when query.Latitude.HasValue && query.Longitude.HasValue =>
                filteredListings.OrderBy(l => CalculateDistance(query.Latitude.Value, query.Longitude.Value, l.Latitude, l.Longitude)),

            _ => filteredListings.OrderByDescending(l => l.Rating)
        };

        var pagedListings = filteredListings
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

        var listingResponses = _mapper.Map<IEnumerable<ListingResponse>>(pagedListings);

        var allFiltered = filteredListings.ToList();
        var metadata = new SearchMetadata(
            TotalResults: totalCount,
            TypeCounts: allFiltered
                .GroupBy(l => l.Type.ToString())
                .ToDictionary(g => g.Key, g => g.Count()),
            AveragePrice: allFiltered.Any() ? allFiltered.Average(l => l.BasePricePerDay) : null,
            PriceRange: allFiltered.Any()
                ? new PriceRange(allFiltered.Min(l => l.BasePricePerDay), allFiltered.Max(l => l.BasePricePerDay))
                : null
        );

        return new SearchListingsResponse(
            Results: new PagedResult<ListingResponse>
            {
                Items = listingResponses,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalCount = totalCount
            },
            Metadata: metadata
        );
    }

    public async Task<PagedResult<CustomerSearchListingItemDto>> SearchCustomerListingsAsync(
        CustomerSearchListingsQuery query,
        Guid? userId,
        CancellationToken cancellationToken = default)
    {
        var safePage = Math.Max(1, query.Page);
        var safePageSize = Math.Clamp(query.PageSize, 1, 100);

        var listings = (await _listingRepository.GetActiveListingsAsync(cancellationToken)).ToList();
        IEnumerable<Listing> filtered = listings;

        if (TryParseViewType(query.ViewType, out var viewType))
        {
            filtered = filtered.Where(x => x.Type == viewType);
        }
        else
        {
            filtered = filtered.Where(x => x.Type == ListingType.Beach || x.Type == ListingType.Pool);
        }

        if (!string.IsNullOrWhiteSpace(query.City))
        {
            var city = query.City.Trim();
            filtered = filtered.Where(x => x.City.Contains(city, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var term = query.SearchTerm.Trim();
            filtered = filtered.Where(x =>
                x.City.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                x.Country.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                x.Title.Values.Any(v => v.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                x.Description.Values.Any(v => v.Contains(term, StringComparison.OrdinalIgnoreCase)));
        }

        if (query.Guests.HasValue)
        {
            filtered = filtered.Where(x => x.MaxGuestCount >= query.Guests.Value);
        }

        if (query.MinPrice.HasValue)
        {
            filtered = filtered.Where(x => x.BasePricePerDay >= query.MinPrice.Value);
        }

        if (query.MaxPrice.HasValue)
        {
            filtered = filtered.Where(x => x.BasePricePerDay <= query.MaxPrice.Value);
        }

        var amenityTokens = SplitCsv(query.Amenities)
            .Select(NormalizeToken)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (amenityTokens.Count > 0)
        {
            filtered = filtered.Where(x =>
            {
                var listingAmenityTokens = GetListingAmenityTokens(x);
                return amenityTokens.All(token => listingAmenityTokens.Contains(token));
            });
        }

        var sortBy = (query.SortBy ?? "recommended").Trim().ToLowerInvariant();
        var sortDescending = (query.SortOrder ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "asc" => false,
            "desc" => true,
            _ => sortBy != "price"
        };

        filtered = sortBy switch
        {
            "price" => sortDescending
                ? filtered.OrderByDescending(x => x.BasePricePerDay)
                : filtered.OrderBy(x => x.BasePricePerDay),
            "rating" => sortDescending
                ? filtered.OrderByDescending(x => x.Rating)
                : filtered.OrderBy(x => x.Rating),
            "distance" => filtered.OrderBy(x => x.City),
            _ => filtered
                .OrderByDescending(x => x.IsFeatured)
                .ThenByDescending(x => x.Rating)
                .ThenByDescending(x => x.ReviewCount)
        };

        var filteredList = filtered.ToList();
        var totalCount = filteredList.Count;

        var favoriteVenueIds = new HashSet<Guid>();
        if (userId.HasValue)
        {
            var favorites = await _favoriteRepository.GetByUserIdAsync(userId.Value, cancellationToken);
            favoriteVenueIds = favorites.Select(x => x.VenueId).ToHashSet();
        }

        var paged = filteredList
            .Skip((safePage - 1) * safePageSize)
            .Take(safePageSize)
            .Select(x => MapCustomerListing(
                x,
                favoriteVenueIds.Contains(x.Id),
                query.DateFrom,
                query.DateTo,
                query.Guests))
            .ToList();

        return new PagedResult<CustomerSearchListingItemDto>
        {
            Items = paged,
            Page = safePage,
            PageSize = safePageSize,
            TotalCount = totalCount
        };
    }

    public async Task<IEnumerable<string>> GetSearchSuggestionsAsync(string term, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
            return Enumerable.Empty<string>();

        var listings = await _listingRepository.GetActiveListingsAsync(cancellationToken);
        var termLower = term.ToLowerInvariant();

        var titleSuggestions = listings
            .SelectMany(l => l.Title.Values)
            .Where(title => title.Contains(term, StringComparison.OrdinalIgnoreCase));

        var citySuggestions = listings
            .Select(l => $"{l.City}, {l.Country}")
            .Where(x => x.ToLowerInvariant().Contains(termLower));

        return titleSuggestions
            .Concat(citySuggestions)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .Take(10)
            .ToList();
    }

    private static CustomerSearchListingItemDto MapCustomerListing(
        Listing listing,
        bool isFavorite,
        DateTime? dateFrom,
        DateTime? dateTo,
        int? guests)
    {
        var priceUnit = listing.Type == ListingType.Pool ? "person" : "person";
        var displayPrice = $"{GetCurrencySymbol(listing.PriceCurrency)}{listing.BasePricePerDay:0.##}";
        var image = listing.Images.OrderBy(x => x.DisplayOrder).FirstOrDefault(x => x.IsCover)
            ?? listing.Images.OrderBy(x => x.DisplayOrder).FirstOrDefault();

        var tags = listing.Amenities
            .Select(x => GetLocalizedText(x.Amenity?.Label))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Take(3)
            .ToList();

        var badges = new List<string>();
        if (listing.IsFeatured)
        {
            badges.Add("Featured");
        }

        return new CustomerSearchListingItemDto(
            Id: listing.Id,
            Slug: listing.Slug,
            Title: GetLocalizedText(listing.Title),
            ImageUrl: image?.Url,
            ImageAlt: image?.Alt,
            Type: listing.Type.ToString(),
            TypeLabel: listing.Type == ListingType.Beach ? "Beach" : "Pool",
            TypeIcon: listing.Type == ListingType.Beach ? "public" : "pool",
            LocationText: string.IsNullOrWhiteSpace(listing.Country)
                ? listing.City
                : $"{listing.City}, {listing.Country}",
            DistanceKm: null,
            Rating: listing.Rating,
            ReviewCount: listing.ReviewCount,
            Price: listing.BasePricePerDay,
            Currency: listing.PriceCurrency,
            PriceUnit: priceUnit,
            DisplayPrice: displayPrice,
            DisplayPriceUnit: $"/ {priceUnit}",
            DisplayTotal: ResolveDisplayTotal(listing, dateFrom, dateTo, guests),
            Badges: badges,
            Tags: tags,
            IsFavorite: isFavorite,
            Latitude: listing.Latitude,
            Longitude: listing.Longitude
        );
    }

    private static string? ResolveDisplayTotal(Listing listing, DateTime? dateFrom, DateTime? dateTo, int? guests)
    {
        if (!dateFrom.HasValue || !dateTo.HasValue || !guests.HasValue)
        {
            return null;
        }

        var dayCount = Math.Max(1, (dateTo.Value.Date - dateFrom.Value.Date).Days);
        var total = listing.BasePricePerDay * guests.Value * dayCount;
        return $"{GetCurrencySymbol(listing.PriceCurrency)}{total:0.##}";
    }

    private static HashSet<string> GetListingAmenityTokens(Listing listing)
    {
        return listing.Amenities
            .SelectMany(amenity => new[]
            {
                amenity.AmenityId.ToString(),
                NormalizeToken(GetLocalizedText(amenity.Amenity?.Label))
            })
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private static IEnumerable<string> SplitCsv(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        return value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    private static bool TryParseViewType(string? viewType, out ListingType listingType)
    {
        listingType = default;
        if (string.IsNullOrWhiteSpace(viewType))
        {
            return false;
        }

        if (viewType.Equals("beach", StringComparison.OrdinalIgnoreCase))
        {
            listingType = ListingType.Beach;
            return true;
        }

        if (viewType.Equals("pool", StringComparison.OrdinalIgnoreCase))
        {
            listingType = ListingType.Pool;
            return true;
        }

        return false;
    }

    private static string NormalizeToken(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return value.Trim().ToLowerInvariant().Replace(" ", string.Empty);
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

    private static string GetCurrencySymbol(string currency)
    {
        return currency.ToUpperInvariant() switch
        {
            "USD" => "$",
            "EUR" => "€",
            "TRY" => "₺",
            _ => string.Empty
        };
    }

    private double CalculateDistance(decimal lat1, decimal lon1, decimal lat2, decimal lon2)
    {
        const double radius = 6371;
        var dLat = ToRadians((double)(lat2 - lat1));
        var dLon = ToRadians((double)(lon2 - lon1));

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians((double)lat1)) * Math.Cos(ToRadians((double)lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return radius * c;
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;
}
