using Microsoft.AspNetCore.Mvc;
using Swimago.Application.DTOs.Search;
using Swimago.Application.Interfaces;

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
    private readonly ILogger<SearchController> _logger;

    public SearchController(ISearchService searchService, ILogger<SearchController> logger)
    {
        _searchService = searchService;
        _logger = logger;
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
}
