using Microsoft.AspNetCore.Mvc;
using Swimago.Application.DTOs.Destinations;
using Swimago.Domain.Entities;
using Swimago.Domain.Enums;
using Swimago.Domain.Interfaces;

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
    private readonly IReservationRepository _reservationRepository;
    private readonly ILogger<SpotsController> _logger;

    public SpotsController(
        IListingRepository listingRepository,
        IUserRepository userRepository,
        IReservationRepository reservationRepository,
        ILogger<SpotsController> logger)
    {
        _listingRepository = listingRepository;
        _userRepository = userRepository;
        _reservationRepository = reservationRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get spot details by slug (beach or pool)
    /// </summary>
    [HttpGet("{slug}")]
    [ProducesResponseType(typeof(CustomerSpotDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching spot detail: {Slug}", slug);

        var listing = await _listingRepository.GetBySlugAsync(slug, cancellationToken);

        if (listing == null || (listing.Type != ListingType.Beach && listing.Type != ListingType.Pool))
            return NotFound(new { error = "Mekan bulunamadı" });

        var host = await _userRepository.GetByIdAsync(listing.HostId, cancellationToken);

        var gallery = listing.Images
            .OrderBy(x => x.DisplayOrder)
            .Select((x, index) => new SpotGalleryItemDto(
                Url: x.Url,
                Alt: x.Alt,
                IsPrimary: x.IsCover || index == 0))
            .ToList();

        var conditions = (listing.Conditions ?? [])
            .Select(x => new SpotConditionItemDto(
                Icon: x.Icon,
                Label: x.Label,
                Value: x.Value))
            .ToList();

        var amenities = listing.Amenities
            .Select(x => new SpotAmenityAvailabilityDto(
                Icon: x.Amenity?.Icon ?? "check_circle",
                Label: GetLocalizedText(x.Amenity?.Label),
                Available: true))
            .ToList();

        var latestReviews = listing.Reviews
            .OrderByDescending(x => x.CreatedAt)
            .Take(3)
            .ToList();

        var breakdown = Enumerable.Range(1, 5)
            .Reverse()
            .Select(stars =>
            {
                var count = listing.Reviews.Count(x => x.Rating == stars);
                var percentage = listing.Reviews.Count == 0 ? 0 : (int)Math.Round((count * 100d) / listing.Reviews.Count);
                return new SpotReviewBreakdownDto(stars, percentage);
            })
            .ToList();

        var categoryScores = new List<SpotReviewCategoryDto>();
        if (listing.Reviews.Any(x => x.Categories != null))
        {
            var categorized = listing.Reviews.Where(x => x.Categories != null).ToList();
            categoryScores.Add(new SpotReviewCategoryDto("Cleanliness", Math.Round((decimal)categorized.Average(x => x.Categories!.Cleanliness), 1)));
            categoryScores.Add(new SpotReviewCategoryDto("Facilities", Math.Round((decimal)categorized.Average(x => x.Categories!.Facilities), 1)));
            categoryScores.Add(new SpotReviewCategoryDto("Service", Math.Round((decimal)categorized.Average(x => x.Categories!.Service), 1)));
        }

        var defaultGuests = 2;
        var baseTotal = listing.BasePricePerDay * defaultGuests;
        var serviceFee = Math.Round(baseTotal * 0.10m, 2);

        var response = new CustomerSpotDetailResponse(
            Id: listing.Id,
            Slug: listing.Slug,
            Type: listing.Type.ToString(),
            Header: new SpotHeaderDto(
                Title: GetLocalizedText(listing.Title),
                Rating: listing.Rating,
                ReviewCount: listing.ReviewCount,
                Location: BuildLocationText(listing),
                Breadcrumbs:
                [
                    new SpotBreadcrumbDto(listing.Country, "/explore"),
                    new SpotBreadcrumbDto(listing.City, "/explore"),
                    new SpotBreadcrumbDto(GetLocalizedText(listing.Title), null)
                ]),
            Gallery: gallery,
            Conditions: conditions,
            Description: GetLocalizedText(listing.Description),
            Amenities: amenities,
            Location: new SpotLocationDto(
                Name: GetLocalizedText(listing.Title),
                Subtitle: BuildLocationText(listing),
                Latitude: listing.Latitude,
                Longitude: listing.Longitude,
                MapImageUrl: null),
            ReviewsPreview: new SpotReviewsPreviewDto(
                OverallRating: listing.Rating,
                TotalReviews: listing.ReviewCount,
                Breakdown: breakdown,
                Categories: categoryScores,
                Reviews: latestReviews.Select(x => new SpotReviewPreviewItemDto(
                    Id: x.Id,
                    AvatarUrl: x.Guest?.Profile?.Avatar,
                    Name: GetFullName(x.Guest),
                    Date: x.CreatedAt,
                    Text: x.Text)).ToList()),
            BookingDefaults: new SpotBookingDefaultsDto(
                Price: listing.BasePricePerDay,
                Currency: listing.PriceCurrency,
                PriceUnit: "day",
                DefaultDate: DateTime.UtcNow.Date.AddDays(1),
                DefaultGuests: defaultGuests,
                LineItems:
                [
                    new SpotLineItemDto($"Ticket ({defaultGuests}x)", baseTotal),
                    new SpotLineItemDto("Service fee", serviceFee)
                ],
                Total: baseTotal + serviceFee,
                RareFindMessage: listing.ReviewCount > 30 ? "Usually fully booked" : null)
        );

        return Ok(response);
    }

    /// <summary>
    /// Get live quote for selected date and guest selections
    /// </summary>
    [HttpPost("{slug}/quote")]
    [ProducesResponseType(typeof(SpotQuoteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetQuote(string slug, [FromBody] SpotQuoteRequest request, CancellationToken cancellationToken)
    {
        if (request.Guests.Adults < 1)
        {
            return BadRequest(new { error = "En az 1 yetişkin seçilmelidir." });
        }

        var listing = await _listingRepository.GetBySlugAsync(slug, cancellationToken);
        if (listing == null || (listing.Type != ListingType.Beach && listing.Type != ListingType.Pool))
        {
            return NotFound(new { error = "Mekan bulunamadı" });
        }

        var totalGuestCount = request.Guests.Adults + request.Guests.Children;
        if (totalGuestCount > listing.MaxGuestCount)
        {
            return BadRequest(new { error = $"Maksimum misafir sayısı {listing.MaxGuestCount}" });
        }

        var bookingDate = request.Date.Date;
        var start = DateTime.SpecifyKind(bookingDate.AddHours(9), DateTimeKind.Utc);
        var end = DateTime.SpecifyKind(bookingDate.AddHours(18), DateTimeKind.Utc);

        var isAvailable = !await _reservationRepository.HasOverlappingReservationsAsync(
            listing.Id,
            start,
            end,
            null,
            cancellationToken);

        var baseAmount = listing.BasePricePerDay * totalGuestCount;

        var selectedAmenities = request.Selections?.SelectedAmenities?.ToList() ?? [];
        var amenityAmount = selectedAmenities.Count * Math.Round(listing.BasePricePerDay * 0.15m, 2);
        var serviceFee = Math.Round((baseAmount + amenityAmount) * 0.10m, 2);

        var lineItems = new List<SpotLineItemDto>
        {
            new($"Entry fee ({totalGuestCount}x)", baseAmount)
        };

        if (amenityAmount > 0)
        {
            lineItems.Add(new SpotLineItemDto("Selected amenities", amenityAmount));
        }

        lineItems.Add(new SpotLineItemDto("Service fee", serviceFee));

        var total = lineItems.Sum(x => x.Amount);

        return Ok(new SpotQuoteResponse(
            Currency: listing.PriceCurrency,
            LineItems: lineItems,
            Total: total,
            IsAvailable: isAvailable,
            UnavailableReason: isAvailable ? null : "Selected date is fully booked"
        ));
    }

    private static string BuildLocationText(Listing listing)
    {
        return string.IsNullOrWhiteSpace(listing.Country)
            ? listing.City
            : $"{listing.City}, {listing.Country}";
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

    private static string GetFullName(User? user)
    {
        if (user?.Profile == null)
        {
            return "Guest";
        }

        var firstName = GetLocalizedText(user.Profile.FirstName);
        var lastName = GetLocalizedText(user.Profile.LastName);
        var fullName = $"{firstName} {lastName}".Trim();

        return string.IsNullOrWhiteSpace(fullName) ? "Guest" : fullName;
    }
}
