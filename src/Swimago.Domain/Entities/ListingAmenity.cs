namespace Swimago.Domain.Entities;

public class ListingAmenity
{
    public Guid ListingId { get; set; }
    public Guid AmenityId { get; set; }
    public bool IsEnabled { get; set; } = true;
    
    // Navigation properties
    public Listing Listing { get; set; } = null!;
    public Amenity Amenity { get; set; } = null!;
}
