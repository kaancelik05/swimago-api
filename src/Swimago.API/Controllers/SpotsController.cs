using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Swimago.Application.DTOs.Common;
using Swimago.Application.DTOs.Destinations;
using Swimago.Domain.Interfaces;
using Swimago.Domain.Enums;

namespace Swimago.API.Controllers;

/// <summary>
/// Spot (beach/pool) detail endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class SpotsController : ControllerBase
{
    private readonly IListingRepository _listingRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<SpotsController> _logger;

    public SpotsController(
        IListingRepository listingRepository,
        IUserRepository userRepository,
        ILogger<SpotsController> logger)
    {
        _listingRepository = listingRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get spot details by slug (beach or pool)
    /// </summary>
    [HttpGet("{slug}")]
    [ProducesResponseType(typeof(SpotDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching spot: {Slug}", slug);

        var listing = await _listingRepository.GetBySlugAsync(slug, cancellationToken);
        
        if (listing == null)
            return NotFound(new { error = "Mekan bulunamadı" });

        if (listing.Type != ListingType.Beach && listing.Type != ListingType.Pool)
            return NotFound(new { error = "Mekan bulunamadı" });

        var host = await _userRepository.GetByIdAsync(listing.HostId, cancellationToken);

        var hostListings = host != null 
            ? await _listingRepository.GetByHostIdAsync(listing.HostId, cancellationToken)
            : Enumerable.Empty<Domain.Entities.Listing>();

        var response = new SpotDetailResponse(
            Id: listing.Id,
            Slug: listing.Slug,
            Name: MultiLanguageDto.FromDictionary(listing.Title),
            Description: MultiLanguageDto.FromDictionary(listing.Description),
            VenueType: listing.Type == ListingType.Beach ? VenueType.Beach : VenueType.Pool,
            Status: listing.Status.ToString(),
            City: listing.City,
            Country: listing.Country,
            Latitude: listing.Latitude,
            Longitude: listing.Longitude,
            BasePricePerDay: listing.BasePricePerDay,
            PriceRangeMin: listing.PriceRangeMin,
            PriceRangeMax: listing.PriceRangeMax,
            Currency: listing.PriceCurrency,
            MaxGuestCount: listing.MaxGuestCount,
            Rating: listing.Rating,
            ReviewCount: listing.ReviewCount,
            IsFeatured: listing.IsFeatured,
            Images: listing.Images.OrderBy(i => i.DisplayOrder).Select(i => new SpotImageDto(
                Id: i.Id,
                Url: i.Url,
                IsCover: i.IsCover,
                DisplayOrder: i.DisplayOrder,
                AltText: i.Alt
            )),
            Amenities: listing.Amenities.Select(la => new SpotAmenityDto(
                Id: la.Amenity?.Id ?? Guid.Empty,
                Name: la.Amenity?.Label.GetValueOrDefault("tr") ?? "",
                Icon: la.Amenity?.Icon,
                Category: la.Amenity?.Category
            )),
            Conditions: null,
            Host: new HostInfoDto(
                Id: listing.HostId,
                Name: host?.Profile != null 
                    ? $"{host.Profile.FirstName.GetValueOrDefault("tr")} {host.Profile.LastName.GetValueOrDefault("tr")}".Trim()
                    : host?.Email ?? "Host",
                Avatar: host?.Profile?.Avatar,
                MemberSince: host?.CreatedAt ?? DateTime.UtcNow,
                ListingCount: hostListings.Count(),
                AverageRating: hostListings.Any() ? hostListings.Average(l => l.Rating) : 0,
                ResponseRate: 95,
                ResponseTime: "1 saat içinde"
            ),
            AvailableSlots: null
        );

        return Ok(response);
    }
}
