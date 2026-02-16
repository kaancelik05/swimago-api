using NetTopologySuite.Geometries;

namespace Swimago.Domain.Entities;

public class Destination
{
    public Guid Id { get; set; }
    
    // Core Info
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    
    // Content
    public string Description { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    
    // Images
    public string ImageUrl { get; set; } = string.Empty;
    public string? MapImageUrl { get; set; }
    
    // Location (PostGIS)
    public Point? Location { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    
    // Stats & Info
    public string? AvgWaterTemp { get; set; }
    public int? SunnyDaysPerYear { get; set; }
    public double? AverageRating { get; set; }
    public int SpotCount { get; set; }
    
    // JSONB Fields
    public List<string> Tags { get; set; } = new();
    public List<DestinationFeature> Features { get; set; } = new();
    
    // Status
    public bool IsFeatured { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Metadata
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class DestinationFeature
{
    public string Icon { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
