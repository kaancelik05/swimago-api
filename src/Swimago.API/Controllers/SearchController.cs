using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Swimago.Application.DTOs.Search;
using Swimago.Application.Interfaces;
using Swimago.Domain.Enums;
using Swimago.Domain.Interfaces;

namespace Swimago.API.Controllers;

/// <summary>
/// Advanced search and filtering for listings
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class SearchController : ControllerBase
{
    private readonly ISearchService _searchService;
    private readonly IAmenityRepository _amenityRepository;
    private readonly ILogger<SearchController> _logger;

    public SearchController(
        ISearchService searchService,
        IAmenityRepository amenityRepository,
        ILogger<SearchController> logger)
    {
        _searchService = searchService;
        _amenityRepository = amenityRepository;
        _logger = logger;
    }

    /// <summary>
    /// Search listings for customer explore screens
    /// </summary>
    /// <param name="query">Customer search query parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated listing cards for customer app</returns>
    [HttpGet("listings")]
    [ProducesResponseType(typeof(Swimago.Application.DTOs.Common.PagedResult<CustomerSearchListingItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchCustomerListings([FromQuery] CustomerSearchListingsQuery query, CancellationToken cancellationToken)
    {
        if (query.Page <= 0)
            return BadRequest(new { error = "page 1 veya daha büyük olmalıdır." });

        if (query.PageSize <= 0 || query.PageSize > 100)
            return BadRequest(new { error = "pageSize 1-100 aralığında olmalıdır." });

        if (query.MinPrice.HasValue && query.MaxPrice.HasValue && query.MinPrice > query.MaxPrice)
            return BadRequest(new { error = "Minimum fiyat, maksimum fiyattan büyük olamaz." });

        Guid? userId = null;
        if (User.Identity?.IsAuthenticated == true &&
            Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var parsedUserId))
        {
            userId = parsedUserId;
        }

        var response = await _searchService.SearchCustomerListingsAsync(query, userId, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Get active amenities for customer filter chips
    /// </summary>
    /// <param name="viewType">Optional view type filter (Beach|Pool)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Active amenity list for customer search filters</returns>
    [HttpGet("amenities")]
    [ProducesResponseType(typeof(CustomerAmenityListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCustomerAmenities([FromQuery] string? viewType, CancellationToken cancellationToken)
    {
        var amenities = (await _amenityRepository.GetActiveAsync(cancellationToken)).ToList();

        if (TryParseViewType(viewType, out var listingType))
        {
            amenities = amenities
                .Where(x => x.ApplicableTo == null || x.ApplicableTo.Count == 0 || x.ApplicableTo.Contains(listingType))
                .ToList();
        }

        var items = amenities
            .Select(x => new CustomerAmenityItemDto(
                Id: x.Id,
                Name: GetLocalizedText(x.Label),
                Icon: x.Icon,
                Category: x.Category))
            .Where(x => !string.IsNullOrWhiteSpace(x.Name))
            .OrderBy(x => x.Name)
            .ToList();

        return Ok(new CustomerAmenityListResponse(items, items.Count));
    }

    /// <summary>
    /// Search and filter listings with advanced criteria
    /// </summary>
    /// <param name="query">Search parameters including filters, location, and sorting</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated search results with metadata</returns>
    /// <response code="200">Returns search results with faceted metadata</response>
    /// <response code="400">Invalid search parameters</response>
    [HttpPost("listings")]
    [ProducesResponseType(typeof(SearchListingsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchListings([FromBody] SearchListingsQuery query, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Search request: Type={Type}, Term={SearchTerm}, Location={Lat},{Lng}", 
            query.Type, query.SearchTerm, query.Latitude, query.Longitude);

        // Validate location parameters
        if (query.Latitude.HasValue || query.Longitude.HasValue || query.RadiusKm.HasValue)
        {
            if (!query.Latitude.HasValue || !query.Longitude.HasValue || !query.RadiusKm.HasValue)
            {
                return BadRequest(new { error = "Konum tabanlı arama için enlem, boylam ve yarıçap belirtilmelidir." });
            }

            if (query.Latitude < -90 || query.Latitude > 90)
                return BadRequest(new { error = "Geçersiz enlem. -90 ile 90 arasında olmalıdır." });

            if (query.Longitude < -180 || query.Longitude > 180)
                return BadRequest(new { error = "Geçersiz boylam. -180 ile 180 arasında olmalıdır." });

            if (query.RadiusKm <= 0 || query.RadiusKm > 500)
                return BadRequest(new { error = "Yarıçap 0 ile 500 km arasında olmalıdır." });
        }

        // Validate price range
        if (query.MinPrice.HasValue && query.MaxPrice.HasValue && query.MinPrice > query.MaxPrice)
        {
            return BadRequest(new { error = "Minimum fiyat, maksimum fiyattan büyük olamaz." });
        }

        var response = await _searchService.SearchListingsAsync(query, cancellationToken);

        _logger.LogInformation("Search completed: {TotalResults} results found", response.Metadata.TotalResults);

        return Ok(response);
    }

    /// <summary>
    /// Get search suggestions based on partial input (autocomplete)
    /// </summary>
    /// <param name="term">Search term (minimum 2 characters)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of suggested search terms</returns>
    /// <response code="200">Returns search suggestions</response>
    [HttpGet("suggestions")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSuggestions([FromQuery] string term, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
        {
            return Ok(Enumerable.Empty<string>());
        }

        var suggestions = await _searchService.GetSearchSuggestionsAsync(term, cancellationToken);
        return Ok(suggestions);
    }

    private static bool TryParseViewType(string? value, out ListingType listingType)
    {
        listingType = default;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        if (value.Equals("beach", StringComparison.OrdinalIgnoreCase))
        {
            listingType = ListingType.Beach;
            return true;
        }

        if (value.Equals("pool", StringComparison.OrdinalIgnoreCase))
        {
            listingType = ListingType.Pool;
            return true;
        }

        return false;
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
