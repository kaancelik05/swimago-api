namespace Swimago.Domain.Entities;

public class HostListingMetadata
{
    public Guid Id { get; set; }
    public Guid ListingId { get; set; }
    public List<string> Highlights { get; set; } = new();
    public List<HostSeatingArea> SeatingAreas { get; set; } = new();
    public string? AvailabilityNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Listing Listing { get; set; } = null!;
}

public class HostSeatingArea
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public decimal PriceMultiplier { get; set; }
    public bool IsVip { get; set; }
    public decimal? MinSpend { get; set; }
}
