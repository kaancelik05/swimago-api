namespace Swimago.Domain.Entities;

public class DailyPricing
{
    public Guid Id { get; set; }
    public Guid ListingId { get; set; }
    public DateOnly Date { get; set; }
    public decimal Price { get; set; }
    public decimal? HourlyPrice { get; set; }
    public bool IsAvailable { get; set; }
    public string? Label { get; set; } // e.g., "High Season", "Weekend Rate"
    public string? Notes { get; set; }
    
    // Navigation property
    public Listing Listing { get; set; } = null!;
}
