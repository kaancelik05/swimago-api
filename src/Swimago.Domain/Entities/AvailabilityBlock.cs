namespace Swimago.Domain.Entities;

public class AvailabilityBlock
{
    public Guid Id { get; set; }
    public Guid ListingId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public bool IsAvailable { get; set; }
    public string? Reason { get; set; } // e.g., "Maintenance", "Private Event"
    public decimal? CustomPrice { get; set; }
    
    // Navigation property
    public Listing Listing { get; set; } = null!;
}
