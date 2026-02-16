using AutoMapper;
using Swimago.Application.DTOs.Common;
using Swimago.Application.DTOs.Listings;
using Swimago.Application.DTOs.Search;
using Swimago.Application.Interfaces;
using Swimago.Domain.Enums;
using Swimago.Domain.Interfaces;

namespace Swimago.Application.Services;

public class SearchService : ISearchService
{
    private readonly IListingRepository _listingRepository;
    private readonly IMapper _mapper;

    public SearchService(IListingRepository listingRepository, IMapper mapper)
    {
        _listingRepository = listingRepository;
        _mapper = mapper;
    }

    public async Task<SearchListingsResponse> SearchListingsAsync(SearchListingsQuery query, CancellationToken cancellationToken = default)
    {
        // Get all active listings
        var allListings = await _listingRepository.GetActiveListingsAsync(cancellationToken);
        var filteredListings = allListings.AsQueryable();

        // Apply filters
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

        // City filter
        if (!string.IsNullOrWhiteSpace(query.City))
        {
            filteredListings = filteredListings.Where(l => l.City.ToLower().Contains(query.City.ToLower()));
        }

        // Full-text search on multi-language fields
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

        // Amenities filter (must have all specified amenities)
        if (query.AmenityIds != null && query.AmenityIds.Any())
        {
            filteredListings = filteredListings.Where(l =>
                query.AmenityIds.All(amenityId =>
                    l.Amenities.Any(la => la.AmenityId == amenityId)
                )
            );
        }

        // Location-based filtering (simplified - in real scenario use PostGIS distance)
        if (query.Latitude.HasValue && query.Longitude.HasValue && query.RadiusKm.HasValue)
        {
            filteredListings = filteredListings.Where(l =>
                CalculateDistance(query.Latitude.Value, query.Longitude.Value, l.Latitude, l.Longitude) <= (double)query.RadiusKm.Value
            );
        }

        // Get total count before pagination
        var totalCount = filteredListings.Count();

        // Sorting
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
            
            _ => filteredListings.OrderByDescending(l => l.Rating) // Default sort by rating
        };

        // Pagination
        var pagedListings = filteredListings
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

        // Map to DTOs
        var listingResponses = _mapper.Map<IEnumerable<ListingResponse>>(pagedListings);

        // Calculate metadata
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

    public async Task<IEnumerable<string>> GetSearchSuggestionsAsync(string term, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
            return Enumerable.Empty<string>();

        var listings = await _listingRepository.GetActiveListingsAsync(cancellationToken);
        var termLower = term.ToLower();

        var suggestions = listings
            .SelectMany(l => l.Title.Values
                .Where(title => title.ToLower().Contains(termLower))
                .Select(title => title))
            .Distinct()
            .Take(10)
            .ToList();

        return suggestions;
    }

    // Haversine formula for distance calculation (simplified)
    private double CalculateDistance(decimal lat1, decimal lon1, decimal lat2, decimal lon2)
    {
        const double R = 6371; // Earth's radius in km
        var dLat = ToRadians((double)(lat2 - lat1));
        var dLon = ToRadians((double)(lon2 - lon1));
        
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians((double)lat1)) * Math.Cos(ToRadians((double)lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private double ToRadians(double degrees) => degrees * Math.PI / 180;
}
