using Microsoft.AspNetCore.Mvc;
using Swimago.Application.DTOs.Destinations;
using Swimago.Domain.Interfaces;
using Swimago.Domain.Enums;

namespace Swimago.API.Controllers;

/// <summary>
/// Map exploration endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ExploreController : ControllerBase
{
    private readonly IListingRepository _listingRepository;
    private readonly ILogger<ExploreController> _logger;

    public ExploreController(
        IListingRepository listingRepository,
        ILogger<ExploreController> logger)
    {
        _listingRepository = listingRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get map markers for exploration view
    /// </summary>
    /// <param name="neLat">Northeast latitude bound</param>
    /// <param name="neLng">Northeast longitude bound</param>
    /// <param name="swLat">Southwest latitude bound</param>
    /// <param name="swLng">Southwest longitude bound</param>
    /// <param name="type">Optional venue type filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Map markers within bounds</returns>
    /// <response code="200">Returns map markers</response>
    [HttpGet]
    [ProducesResponseType(typeof(ExploreResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExploreData(
        [FromQuery] decimal? neLat,
        [FromQuery] decimal? neLng,
        [FromQuery] decimal? swLat,
        [FromQuery] decimal? swLng,
        [FromQuery] VenueType? type,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching explore data: bounds ({SwLat},{SwLng}) to ({NeLat},{NeLng}), type: {Type}",
            swLat, swLng, neLat, neLng, type);

        var activeListings = await _listingRepository.GetActiveListingsAsync(cancellationToken);

        // Filter by bounds if provided
        if (neLat.HasValue && neLng.HasValue && swLat.HasValue && swLng.HasValue)
        {
            activeListings = activeListings.Where(l =>
                l.Latitude >= swLat.Value && l.Latitude <= neLat.Value &&
                l.Longitude >= swLng.Value && l.Longitude <= neLng.Value);
        }

        // Filter by venue type if provided
        if (type.HasValue)
        {
            var listingType = type.Value switch
            {
                VenueType.Beach => ListingType.Beach,
                VenueType.Pool => ListingType.Pool,
                VenueType.Yacht => ListingType.Yacht,
                VenueType.DayTrip => ListingType.DayTrip,
                _ => ListingType.Beach
            };
            activeListings = activeListings.Where(l => l.Type == listingType);
        }

        var listingsList = activeListings.ToList();

        var markers = listingsList.Select(l => new ExploreMarkerDto(
            Id: l.Id,
            Slug: l.Slug,
            Name: l.Title.GetValueOrDefault("tr") ?? "",
            VenueType: l.Type switch
            {
                ListingType.Beach => VenueType.Beach,
                ListingType.Pool => VenueType.Pool,
                ListingType.Yacht => VenueType.Yacht,
                ListingType.DayTrip => VenueType.DayTrip,
                _ => VenueType.Beach
            },
            Latitude: l.Latitude,
            Longitude: l.Longitude,
            Price: l.BasePricePerDay,
            Currency: l.PriceCurrency,
            Rating: l.Rating,
            ThumbnailUrl: l.Images.FirstOrDefault(i => i.IsCover)?.Url ?? l.Images.FirstOrDefault()?.Url
        )).ToList();

        // Calculate bounds from data if not provided
        ExploreMapBoundsDto bounds;
        if (listingsList.Any())
        {
            bounds = new ExploreMapBoundsDto(
                NorthEastLat: listingsList.Max(l => l.Latitude),
                NorthEastLng: listingsList.Max(l => l.Longitude),
                SouthWestLat: listingsList.Min(l => l.Latitude),
                SouthWestLng: listingsList.Min(l => l.Longitude)
            );
        }
        else
        {
            // Default to Turkey bounds if no data
            bounds = new ExploreMapBoundsDto(
                NorthEastLat: 42.0m,
                NorthEastLng: 45.0m,
                SouthWestLat: 36.0m,
                SouthWestLng: 26.0m
            );
        }

        return Ok(new ExploreResponse(markers, bounds));
    }
}
