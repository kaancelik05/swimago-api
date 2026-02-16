using Swimago.Domain.Enums;

namespace Swimago.Application.DTOs.Listings;

public record CreateListingRequest(
    ListingType Type,
    Dictionary<string, string> Title,
    Dictionary<string, string> Description,
    Dictionary<string, string> Address,
    decimal Latitude,
    decimal Longitude,
    int MaxGuestCount,
    IEnumerable<string> ImageUrls,
    IEnumerable<int>? AmenityIds = null
);
