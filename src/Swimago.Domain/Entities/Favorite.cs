using Swimago.Domain.Enums;

namespace Swimago.Domain.Entities;

public class Favorite
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid VenueId { get; set; } // Can be ListingId for any venue type
    public VenueType VenueType { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public User User { get; set; } = null!;
    public Listing? Listing { get; set; }
}
