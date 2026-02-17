using Swimago.Application.DTOs.Search;
using Swimago.Application.DTOs.Listings;
using Swimago.Application.DTOs.Common;

namespace Swimago.Application.Interfaces;

public interface ISearchService
{
    Task<SearchListingsResponse> SearchListingsAsync(SearchListingsQuery query, CancellationToken cancellationToken = default);
    Task<PagedResult<CustomerSearchListingItemDto>> SearchCustomerListingsAsync(
        CustomerSearchListingsQuery query,
        Guid? userId,
        CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetSearchSuggestionsAsync(string term, CancellationToken cancellationToken = default);
}
