namespace Swimago.Domain.Entities;

public class ListingImage
{
    public Guid Id { get; set; }
    public Guid ListingId { get; set; }
    public string Url { get; set; } = string.Empty; // CDN URL
    public string? Alt { get; set; } // Image alt text
    public int DisplayOrder { get; set; }
    public bool IsCover { get; set; } // Cover/primary image
    
    // Navigation property
    public Listing Listing { get; set; } = null!;
}
