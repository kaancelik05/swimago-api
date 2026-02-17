namespace Swimago.Application.DTOs.Search;

public class CustomerSearchListingsQuery
{
    public string? ViewType { get; set; }
    public string? SearchTerm { get; set; }
    public string? City { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int? Guests { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? Amenities { get; set; }
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public record CustomerSearchListingItemDto(
    Guid Id,
    string Slug,
    string Title,
    string? ImageUrl,
    string? ImageAlt,
    string Type,
    string TypeLabel,
    string TypeIcon,
    string LocationText,
    decimal? DistanceKm,
    decimal Rating,
    int ReviewCount,
    decimal Price,
    string Currency,
    string PriceUnit,
    string DisplayPrice,
    string DisplayPriceUnit,
    string? DisplayTotal,
    IEnumerable<string> Badges,
    IEnumerable<string> Tags,
    bool IsFavorite,
    decimal Latitude,
    decimal Longitude
);
