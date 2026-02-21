using Swimago.Domain.Common;
using Swimago.Domain.Enums;
using NetTopologySuite.Geometries;

namespace Swimago.Domain.Entities;

public class Listing : ISoftDeletable
{
    public Guid Id { get; set; }
    public Guid HostId { get; set; }
    public ListingType Type { get; set; }
    public ListingStatus Status { get; set; } = ListingStatus.Pending;
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    
    // URL-friendly identifier
    public string Slug { get; set; } = string.Empty;
    
    // Multi-language content (will be JSONB in database)
    public Dictionary<string, string> Title { get; set; } = new();
    public Dictionary<string, string> Description { get; set; } = new();
    public Dictionary<string, string> Address { get; set; } = new();
    
    // Location details
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    
    // Geospatial data (PostGIS)
    public Point? Location { get; set; } // NetTopologySuite Point for PostGIS
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    
    // Capacity & Pricing
    public int MaxGuestCount { get; set; }
    public decimal BasePricePerHour { get; set; }
    public decimal BasePricePerDay { get; set; }
    public decimal? PriceRangeMin { get; set; }
    public decimal? PriceRangeMax { get; set; }
    public string PriceCurrency { get; set; } = "USD";
    
    // Conditions (weather, waves, etc.) stored as JSONB
    public List<ListingCondition>? Conditions { get; set; }
    
    // Metadata
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public decimal Rating { get; set; }
    public int ReviewCount { get; set; }
    public int SpotCount { get; set; } // For destination-style listings
    
    // Boat tour specific fields
    public string? Duration { get; set; } // e.g., "4 hours", "Full day"
    public bool IsSuperhost { get; set; }

    // Type-specific details (JSONB)
    // Stores complex data structure for Yacht, DayTrip, etc.
    public string? Details { get; set; }
    
    // Rejection info
    public string? RejectionReason { get; set; }
    
    // Navigation properties
    public User Host { get; set; } = null!;
    public ICollection<ListingImage> Images { get; set; } = new List<ListingImage>();
    public ICollection<DailyPricing> PricingCalendar { get; set; } = new List<DailyPricing>();
    public ICollection<AvailabilityBlock> AvailabilityBlocks { get; set; } = new List<AvailabilityBlock>();
    public ICollection<ListingAmenity> Amenities { get; set; } = new List<ListingAmenity>();
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    public HostListingMetadata? HostMetadata { get; set; }

    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}

// Embedded class for conditions stored as JSONB
public class ListingCondition
{
    public string Icon { get; set; } = string.Empty;
    public string IconColor { get; set; } = string.Empty;
    public string BgColor { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
