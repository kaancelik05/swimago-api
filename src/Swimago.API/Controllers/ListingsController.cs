using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swimago.API.Authorization;
using Swimago.Application.DTOs.Common;
using Swimago.Application.DTOs.Listings;
using Swimago.Domain.Interfaces;
using Swimago.Domain.Enums;
using System.Security.Claims;

namespace Swimago.API.Controllers;

/// <summary>
/// Listing management endpoints for beaches, pools, and boat tours
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ListingsController : ControllerBase
{
    private readonly IListingRepository _listingRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ListingsController> _logger;

    public ListingsController(
        IListingRepository listingRepository,
        IMapper mapper,
        ILogger<ListingsController> logger)
    {
        _listingRepository = listingRepository;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Get all active listings with pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ListingResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationQuery query, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching listings with pagination: Page={Page}, PageSize={PageSize}", 
            query.Page, query.PageSize);

        var allListings = await _listingRepository.GetActiveListingsAsync(cancellationToken);
        var totalCount = allListings.Count();
        
        var pagedListings = allListings
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

        var response = _mapper.Map<IEnumerable<ListingResponse>>(pagedListings);
        
        return Ok(new PagedResult<ListingResponse>
        {
            Items = response,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount
        });
    }

    /// <summary>
    /// Get listing by ID with full details
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ListingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching listing details for ID: {ListingId}", id);

        var listing = await _listingRepository.GetWithDetailsAsync(id, cancellationToken);
        
        if (listing == null)
        {
            _logger.LogWarning("Listing not found: {ListingId}", id);
            return NotFound(new { error = "Listing not found" });
        }

        var response = _mapper.Map<ListingResponse>(listing);
        return Ok(response);
    }

    /// <summary>
    /// Get listings by type (Beach, Pool, BoatTour)
    /// </summary>
    [HttpGet("type/{type}")]
    [ProducesResponseType(typeof(IEnumerable<ListingResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByType(ListingType type, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching listings by type: {Type}", type);

        var listings = await _listingRepository.GetByTypeAsync(type, cancellationToken);
        var response = _mapper.Map<IEnumerable<ListingResponse>>(listings);
        return Ok(response);
    }

    /// <summary>
    /// Search nearby listings using geospatial query (PostGIS)
    /// </summary>
    [HttpGet("nearby")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchNearby(
        [FromQuery] decimal latitude,
        [FromQuery] decimal longitude,
        [FromQuery] decimal radius = 10,
        [FromQuery] ListingType? type = null,
        CancellationToken cancellationToken = default)
    {
        if (latitude < -90 || latitude > 90)
            return BadRequest(new { error = "Geçersiz enlem. -90 ile 90 arasında olmalıdır." });

        if (longitude < -180 || longitude > 180)
            return BadRequest(new { error = "Geçersiz boylam. -180 ile 180 arasında olmalıdır." });

        if (radius <= 0 || radius > 100)
            return BadRequest(new { error = "Geçersiz yarıçap. 0 ile 100 km arasında olmalıdır." });

        _logger.LogInformation("Searching nearby listings: Lat={Lat}, Lng={Lng}, Radius={Radius}km, Type={Type}",
            latitude, longitude, radius, type);

        var listings = await _listingRepository.SearchNearbyAsync(
            latitude, 
            longitude, 
            radius, 
            type, 
            cancellationToken);

        var responseData = _mapper.Map<IEnumerable<ListingResponse>>(listings);

        return Ok(new
        {
            searchCenter = new { latitude, longitude },
            radiusKm = radius,
            type,
            count = responseData.Count(),
            results = responseData
        });
    }

    /// <summary>
    /// Create a new listing (Host only)
    /// </summary>
    [Authorize(Policy = AuthorizationPolicies.HostOnly)]
    [HttpPost]
    [ProducesResponseType(typeof(ListingResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateListingRequest request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        _logger.LogInformation("Creating new listing for host {HostId}", userId);

        // TODO: Implement listing creation logic
        // For now, return a placeholder response
        
        return CreatedAtAction(nameof(GetById), new { id = Guid.NewGuid() }, new { message = "İlan oluşturuldu" });
    }

    /// <summary>
    /// Upload photos for a listing
    /// </summary>
    [Authorize(Policy = AuthorizationPolicies.HostOnly)]
    [HttpPost("photos/upload")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadPhotos(List<IFormFile> files, CancellationToken cancellationToken)
    {
        if (files == null || files.Count == 0)
            return BadRequest(new { error = "Lütfen fotoğraf seçin" });

        var uploadedUrls = new List<string>();
        
        foreach (var file in files)
        {
            // TODO: Upload to storage
            uploadedUrls.Add($"/listings/photos/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}");
        }

        return Ok(uploadedUrls);
    }
}

public record CreateListingRequest(
    string Title, 
    string Description, 
    ListingType Type, 
    decimal PricePerDay,
    string City,
    string Country,
    decimal Latitude,
    decimal Longitude
);
