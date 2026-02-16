using System.ComponentModel.DataAnnotations;
using Swimago.Application.DTOs.Admin.Shared;

namespace Swimago.Application.DTOs.Admin.Destinations;

public class DestinationListItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public int SpotCount { get; set; }
    public double? AverageRating { get; set; }
    public bool IsFeatured { get; set; }
}

public class DestinationDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? MapImageUrl { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? AvgWaterTemp { get; set; }
    public int? SunnyDaysPerYear { get; set; }
    public List<string> Tags { get; set; } = new();
    public bool IsFeatured { get; set; }
    public List<DestinationFeatureDto> Features { get; set; } = new();
}

public class DestinationFeatureDto
{
    public string Icon { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class CreateDestinationRequest
{
    [Required] public string Name { get; set; } = string.Empty;
    [Required] public string Slug { get; set; } = string.Empty;
    [Required] public string Country { get; set; } = string.Empty;
    [Required] public string Description { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    [Required] public string ImageUrl { get; set; } = string.Empty;
    public string? MapImageUrl { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? AvgWaterTemp { get; set; }
    public int? SunnyDaysPerYear { get; set; }
    public List<string> Tags { get; set; } = new();
    public bool IsFeatured { get; set; }
    public List<DestinationFeatureDto> Features { get; set; } = new();
}
