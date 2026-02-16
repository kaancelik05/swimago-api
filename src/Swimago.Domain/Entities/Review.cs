namespace Swimago.Domain.Entities;

public class Review
{
    public Guid Id { get; set; }
    public Guid ReservationId { get; set; }
    public Guid ListingId { get; set; }
    public Guid GuestId { get; set; }
    
    public int Rating { get; set; } // 1-5
    public string Text { get; set; } = string.Empty;
    
    // Category ratings
    public ReviewCategories? Categories { get; set; }
    
    // Host response
    public string? HostResponseText { get; set; }
    public DateTime? HostResponseDate { get; set; }
    
    // Metadata
    public DateTime CreatedAt { get; set; }
    public bool IsVerified { get; set; } // Verified purchase
    
    // Navigation properties
    public Reservation Reservation { get; set; } = null!;
    public Listing Listing { get; set; } = null!;
    public User Guest { get; set; } = null!;
}

// Embedded class for category ratings
public class ReviewCategories
{
    public int Cleanliness { get; set; }
    public int Facilities { get; set; }
    public int Service { get; set; }
}
