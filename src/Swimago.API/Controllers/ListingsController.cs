using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swimago.API.Authorization;
using Swimago.Application.DTOs.Common;
using Swimago.Application.DTOs.Listings;
using Swimago.Domain.Entities;
using Swimago.Domain.Enums;
using Swimago.Domain.Interfaces;
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
    private readonly IAmenityRepository _amenityRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ListingsController> _logger;

    public ListingsController(
        IListingRepository listingRepository,
        IAmenityRepository amenityRepository,
        IUnitOfWork unitOfWork,
        ILogger<ListingsController> logger)
    {
        _listingRepository = listingRepository;
        _amenityRepository = amenityRepository;
        _unitOfWork = unitOfWork;
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

        var response = pagedListings.Select(ToListingResponse).ToList();

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

        var response = ToListingResponse(listing);
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
        var response = listings.Select(ToListingResponse).ToList();
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

        var responseData = listings.Select(ToListingResponse).ToList();

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
    /// Create a new listing draft
    /// </summary>
    [Authorize(Policy = AuthorizationPolicies.HostOrAdmin)]
    [HttpPost]
    [ProducesResponseType(typeof(CustomerCreateListingResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CustomerCreateListingRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest(new { error = "title zorunludur" });

        if (!TryParseListingType(request.Type, out var listingType))
            return BadRequest(new { error = "type Beach|Pool|Yacht|DayTrip olmalıdır" });

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        _logger.LogInformation("Creating listing draft for user {UserId}", userId);

        var now = DateTime.UtcNow;
        var slug = await GenerateUniqueSlugAsync(request.Title, cancellationToken);

        var status = ResolveCreateStatus(request.Status);
        var listing = new Listing
        {
            Id = Guid.NewGuid(),
            HostId = userId,
            Type = listingType,
            Status = status,
            IsActive = status == ListingStatus.Active,
            IsFeatured = false,
            Slug = slug,
            Title = new Dictionary<string, string> { ["tr"] = request.Title.Trim() },
            Description = new Dictionary<string, string> { ["tr"] = request.Description.Trim() },
            Address = new Dictionary<string, string> { ["tr"] = request.City.Trim() },
            City = request.City.Trim(),
            Country = request.Country.Trim(),
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            MaxGuestCount = 20,
            BasePricePerHour = request.Pricing.StandardPrice,
            BasePricePerDay = request.Pricing.StandardPrice,
            PriceRangeMin = request.Pricing.ChildSeniorPrice,
            PriceRangeMax = request.Pricing.StandardPrice,
            PriceCurrency = request.Pricing.Currency.Trim().ToUpperInvariant(),
            CreatedAt = now,
            UpdatedAt = now,
            Rating = 0,
            ReviewCount = 0,
            SpotCount = 0,
            IsSuperhost = false
        };

        var activeAmenities = (await _amenityRepository.GetActiveAsync(cancellationToken)).ToList();
        var requestedAmenityTokens = request.Amenities
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(NormalizeToken)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var amenity in activeAmenities)
        {
            var amenityLabel = GetLocalizedText(amenity.Label);
            if (!requestedAmenityTokens.Contains(NormalizeToken(amenityLabel)))
            {
                continue;
            }

            listing.Amenities.Add(new ListingAmenity
            {
                ListingId = listing.Id,
                AmenityId = amenity.Id
            });
        }

        await _listingRepository.AddAsync(listing, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = listing.Id }, new CustomerCreateListingResponse(
            Id: listing.Id,
            Slug: listing.Slug,
            Status: listing.Status.ToString()));
    }

    /// <summary>
    /// Upload photos for a listing
    /// </summary>
    [Authorize(Policy = AuthorizationPolicies.HostOrAdmin)]
    [HttpPost("photos/upload")]
    [ProducesResponseType(typeof(ListingPhotosUploadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadPhotos(
        [FromForm] Guid listingId,
        [FromForm] List<IFormFile> photos,
        CancellationToken cancellationToken)
    {
        if (photos == null || photos.Count == 0)
            return BadRequest(new { error = "Lütfen fotoğraf seçin" });

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var listing = await _listingRepository.GetWithDetailsAsync(listingId, cancellationToken);
        if (listing == null)
            return NotFound(new { error = "İlan bulunamadı" });

        if (listing.HostId != userId)
            return Forbid();

        var uploadedUrls = new List<string>();
        var displayStart = listing.Images.Count;

        for (var i = 0; i < photos.Count; i++)
        {
            var file = photos[i];
            var url = $"/listings/{listingId}/photos/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

            listing.Images.Add(new ListingImage
            {
                Id = Guid.NewGuid(),
                ListingId = listingId,
                Url = url,
                IsCover = listing.Images.Count == 0 && i == 0,
                DisplayOrder = displayStart + i,
                Alt = file.FileName
            });

            uploadedUrls.Add(url);
        }

        await _listingRepository.UpdateAsync(listing, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(new ListingPhotosUploadResponse(uploadedUrls));
    }

    /// <summary>
    /// Publish listing for review
    /// </summary>
    [Authorize(Policy = AuthorizationPolicies.HostOrAdmin)]
    [HttpPost("{id}/publish")]
    [ProducesResponseType(typeof(PublishListingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Publish(Guid id, [FromBody] PublishListingRequest request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var listing = await _listingRepository.GetWithDetailsAsync(id, cancellationToken);
        if (listing == null)
            return NotFound(new { error = "İlan bulunamadı" });

        if (listing.HostId != userId)
            return Forbid();

        if (!request.TermsAccepted)
            return BadRequest(new { error = "Yayınlama için şartlar kabul edilmelidir" });

        if (string.IsNullOrWhiteSpace(request.CoverPhotoUrl))
            return BadRequest(new { error = "coverPhotoUrl zorunludur" });

        foreach (var image in listing.Images)
        {
            image.IsCover = string.Equals(image.Url, request.CoverPhotoUrl, StringComparison.OrdinalIgnoreCase);
        }

        if (!listing.Images.Any(x => x.IsCover))
        {
            listing.Images.Add(new ListingImage
            {
                Id = Guid.NewGuid(),
                ListingId = listing.Id,
                Url = request.CoverPhotoUrl,
                IsCover = true,
                DisplayOrder = listing.Images.Count,
                Alt = listing.Title.GetValueOrDefault("tr")
            });
        }

        listing.Status = ListingStatus.PendingReview;
        listing.IsActive = false;
        listing.UpdatedAt = DateTime.UtcNow;

        await _listingRepository.UpdateAsync(listing, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(new PublishListingResponse(
            Id: listing.Id,
            Status: listing.Status.ToString(),
            Message: "Listing submitted for review"));
    }

    private static ListingResponse ToListingResponse(Listing listing)
    {
        var images = (listing.Images ?? Array.Empty<ListingImage>())
            .OrderBy(x => x.DisplayOrder)
            .Select(x => new ListingImageDto(
                Id: x.Id,
                Url: x.Url,
                IsCover: x.IsCover,
                DisplayOrder: x.DisplayOrder,
                AltText: x.Alt))
            .ToList();

        var amenities = (listing.Amenities ?? Array.Empty<ListingAmenity>())
            .Where(x => x.IsEnabled && x.Amenity != null)
            .Select(x => new AmenityDto(
                Id: x.Amenity!.Id,
                Icon: x.Amenity.Icon,
                Name: MultiLanguageDto.FromDictionary(x.Amenity.Label),
                IsActive: x.Amenity.IsActive))
            .ToList();

        return new ListingResponse(
            Id: listing.Id,
            HostId: listing.HostId,
            Slug: listing.Slug,
            Type: listing.Type,
            Status: listing.Status.ToString(),
            IsActive: listing.IsActive,
            IsFeatured: listing.IsFeatured,
            Title: MultiLanguageDto.FromDictionary(listing.Title),
            Description: MultiLanguageDto.FromDictionary(listing.Description),
            Address: MultiLanguageDto.FromDictionary(listing.Address),
            City: listing.City,
            Country: listing.Country,
            Latitude: listing.Latitude,
            Longitude: listing.Longitude,
            MaxGuestCount: listing.MaxGuestCount,
            BasePricePerDay: listing.BasePricePerDay,
            PriceRangeMin: listing.PriceRangeMin,
            PriceRangeMax: listing.PriceRangeMax,
            Currency: string.IsNullOrWhiteSpace(listing.PriceCurrency) ? "USD" : listing.PriceCurrency,
            Rating: listing.Rating,
            ReviewCount: listing.ReviewCount,
            Images: images,
            Amenities: amenities.Count > 0 ? amenities : null);
    }

    private static string NormalizeToken(string value)
    {
        return value.Trim().ToLowerInvariant().Replace(" ", string.Empty);
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

    private static ListingStatus ResolveCreateStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return ListingStatus.Draft;
        }

        return status.Trim().ToLowerInvariant() switch
        {
            "draft" => ListingStatus.Draft,
            "active" => ListingStatus.Active,
            "pending" => ListingStatus.Pending,
            "pendingreview" => ListingStatus.PendingReview,
            _ => ListingStatus.Draft
        };
    }

    private static bool TryParseListingType(string? value, out ListingType listingType)
    {
        listingType = ListingType.Beach;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        switch (value.Trim().ToLowerInvariant())
        {
            case "beach":
                listingType = ListingType.Beach;
                return true;
            case "pool":
                listingType = ListingType.Pool;
                return true;
            case "yacht":
                listingType = ListingType.Yacht;
                return true;
            case "daytrip":
            case "day-trip":
                listingType = ListingType.DayTrip;
                return true;
            default:
                return false;
        }
    }

    private async Task<string> GenerateUniqueSlugAsync(string title, CancellationToken cancellationToken)
    {
        var baseSlug = title.Trim().ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("ı", "i")
            .Replace("ğ", "g")
            .Replace("ü", "u")
            .Replace("ş", "s")
            .Replace("ö", "o")
            .Replace("ç", "c");

        var slug = baseSlug;
        var suffix = 2;

        while (await _listingRepository.AnyAsync(x => x.Slug == slug, cancellationToken))
        {
            slug = $"{baseSlug}-{suffix}";
            suffix++;
        }

        return slug;
    }
}
