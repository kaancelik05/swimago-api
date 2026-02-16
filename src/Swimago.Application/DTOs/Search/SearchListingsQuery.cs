using Swimago.Domain.Enums;

namespace Swimago.Application.DTOs.Search;

public class SearchListingsQuery
{
    // Location-based search
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public decimal? RadiusKm { get; set; }

    // Basic filters
    public ListingType? Type { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int? MinGuestCount { get; set; }
    public decimal? MinRating { get; set; }

    // City filter
    public string? City { get; set; }

    // Amenities
    public List<Guid>? AmenityIds { get; set; }

    // Search term (multi-language full-text search)
    public string? SearchTerm { get; set; }

    // Sorting
    public SearchSortBy SortBy { get; set; } = SearchSortBy.Relevance;
    public bool SortDescending { get; set; } = false;

    // Pagination
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public enum SearchSortBy
{
    Relevance,      // For text searches
    Price,
    Rating,
    Distance,       // For location-based searches
    CreatedDate,
    ReviewCount
}
