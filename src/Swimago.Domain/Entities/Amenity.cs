using Swimago.Domain.Enums;

namespace Swimago.Domain.Entities;

public class Amenity
{
    public Guid Id { get; set; }
    public string Icon { get; set; } = string.Empty; // e.g., "pool", "wifi", "parking"
    public Dictionary<string, string> Label { get; set; } = new(); // Multi-language
    public string? Category { get; set; }
    public List<ListingType>? ApplicableTo { get; set; } // Which listing types this amenity applies to
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation property
    public ICollection<ListingAmenity> ListingAmenities { get; set; } = new List<ListingAmenity>();
}
