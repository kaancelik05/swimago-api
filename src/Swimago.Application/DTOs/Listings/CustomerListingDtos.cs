namespace Swimago.Application.DTOs.Listings;

public record CustomerCreateListingRequest(
    string Title,
    string Description,
    string Type,
    string City,
    string Country,
    decimal Latitude,
    decimal Longitude,
    CustomerListingPricingRequest Pricing,
    IEnumerable<string> Amenities,
    string? Status
);

public record CustomerListingPricingRequest(
    decimal StandardPrice,
    decimal? ChildSeniorPrice,
    bool SunbedEnabled,
    decimal? SunbedPrice,
    int? SunbedQuantity,
    string Currency
);

public record CustomerCreateListingResponse(
    Guid Id,
    string Slug,
    string Status
);

public record ListingPhotosUploadResponse(
    IEnumerable<string> Urls
);

public record PublishListingRequest(
    string CoverPhotoUrl,
    bool TermsAccepted
);

public record PublishListingResponse(
    Guid Id,
    string Status,
    string Message
);
