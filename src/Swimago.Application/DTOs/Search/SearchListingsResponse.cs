using Swimago.Application.DTOs.Common;
using Swimago.Application.DTOs.Listings;

namespace Swimago.Application.DTOs.Search;

public record SearchListingsResponse(
    PagedResult<ListingResponse> Results,
    SearchMetadata Metadata
);

public record SearchMetadata(
    int TotalResults,
    Dictionary<string, int> TypeCounts,
    decimal? AveragePrice,
    PriceRange? PriceRange
);

public record PriceRange(
    decimal Min,
    decimal Max
);
