using Microsoft.AspNetCore.Mvc;
using Swimago.Application.DTOs.BoatTours;
using Swimago.Application.Interfaces;

namespace Swimago.API.Controllers;

[ApiController]
[Route("api/boat-tours")]
[Produces("application/json")]
public class BoatToursController : ControllerBase
{
    private readonly IBoatTourService _boatTourService;
    private readonly ILogger<BoatToursController> _logger;

    public BoatToursController(
        IBoatTourService boatTourService,
        ILogger<BoatToursController> logger)
    {
        _boatTourService = boatTourService;
        _logger = logger;
    }

    /// <summary>
    /// Get all boat tours (yachts and day trips)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(BoatTourListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? city,
        [FromQuery] string? type,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        CancellationToken cancellationToken)
    {
        var result = await _boatTourService.GetAllBoatToursAsync(city, type, minPrice, maxPrice, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get yacht tour details by slug
    /// </summary>
    [HttpGet("yacht/{slug}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetYachtBySlug(string slug, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _boatTourService.GetYachtTourBySlugAsync(slug, cancellationToken);

            var response = new
            {
                id = result.Id,
                slug = result.Slug,
                title = result.Name.Tr,
                location = BuildLocation(result.City, result.Country),
                rating = result.Rating,
                reviewCount = result.ReviewCount,
                isSuperhost = result.Host.ResponseRate >= 90,
                gallery = result.Images.Select(x => new { url = x.Url, alt = x.AltText }),
                specs = BuildYachtSpecs(result),
                about = SplitParagraphs(result.Description?.Tr),
                features = result.Amenities.Select(x => new { icon = x.Icon, label = x.Name }),
                accommodationOptions = Array.Empty<object>(),
                cateringItems = Array.Empty<object>(),
                activityItems = Array.Empty<object>(),
                cruisingRoute = new
                {
                    name = result.Model ?? "Standard route",
                    stops = result.City,
                    totalRoutes = 1,
                    mapImageUrl = (string?)null
                },
                bookingDefaults = new
                {
                    price = result.PricePerDay,
                    priceUnit = "day",
                    showTripTypeToggle = true,
                    lineItems = new[]
                    {
                        new { label = "Yacht rental", amount = result.PricePerDay },
                        new { label = "Service fee", amount = Math.Round(result.PricePerDay * 0.10m, 2) }
                    },
                    total = result.PricePerDay + Math.Round(result.PricePerDay * 0.10m, 2)
                }
            };

            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get day trip details by slug
    /// </summary>
    [HttpGet("day-trip/{slug}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDayTripBySlug(string slug, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _boatTourService.GetDayTripBySlugAsync(slug, cancellationToken);

            var response = new
            {
                id = result.Id,
                slug = result.Slug,
                title = result.Name.Tr,
                location = BuildLocation(result.City, result.Country),
                rating = result.Rating,
                reviewCount = result.ReviewCount,
                gallery = result.Images.Select(x => new { url = x.Url, alt = x.AltText }),
                infoBadges = new[]
                {
                    new { icon = "schedule", label = "Duration", value = FormatDuration(result.Duration) }
                },
                host = new
                {
                    avatarUrl = result.Host.Avatar,
                    name = result.Host.Name,
                    title = "Professional Captain",
                    experience = "10 years"
                },
                description = SplitParagraphs(result.Description?.Tr),
                routeStops = Array.Empty<object>(),
                amenities = result.Amenities.Select(x => new { icon = x.Icon, label = x.Name }),
                foodItems = Array.Empty<object>(),
                activityTags = result.Amenities.Take(3).Select(x => new { label = x.Name }),
                musicInfo = new { text = "Bluetooth available" },
                bookingDefaults = new
                {
                    price = result.PricePerPerson,
                    priceUnit = "tour",
                    lineItems = new[]
                    {
                        new { label = "Private Charter Base", amount = result.PricePerPerson }
                    },
                    total = result.PricePerPerson
                }
            };

            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    private static string BuildLocation(string? city, string? country)
    {
        if (string.IsNullOrWhiteSpace(city))
        {
            return country ?? string.Empty;
        }

        return string.IsNullOrWhiteSpace(country) ? city : $"{city}, {country}";
    }

    private static IEnumerable<string> SplitParagraphs(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return [];
        }

        return text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    private static string FormatDuration(TimeSpan duration)
    {
        if (duration == TimeSpan.Zero)
        {
            return "Flexible";
        }

        return $"{(int)duration.TotalHours} Hours";
    }

    private static IEnumerable<object> BuildYachtSpecs(YachtTourDetailResponse result)
    {
        var specs = new List<object>
        {
            new { icon = "directions_boat", label = "Type", value = "Motor Yacht" },
            new { icon = "people", label = "Capacity", value = result.Capacity.ToString() }
        };

        if (result.Length.HasValue)
            specs.Add(new { icon = "straighten", label = "Length", value = $"{result.Length.Value:0.#}m" });

        if (!string.IsNullOrWhiteSpace(result.Model))
            specs.Add(new { icon = "star", label = "Model", value = result.Model });

        return specs;
    }
}
