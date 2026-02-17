using Swimago.Domain.Enums;

namespace Swimago.Domain.Entities;

public class Reservation
{
    public Guid Id { get; set; }
    public Guid ListingId { get; set; }
    public Guid GuestId { get; set; }
    
    // Venue type for mixed bookings (beaches, pools, boat tours)
    public VenueType VenueType { get; set; }
    
    // Booking details
    public BookingType BookingType { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    
    // Guest details
    public int GuestCount { get; set; }
    public GuestDetails? Guests { get; set; } // Adults, children, seniors breakdown
    
    // Selections (sunbeds, cabanas, etc.)
    public ReservationSelections? Selections { get; set; }
    
    // Pricing
    public decimal UnitPrice { get; set; } // Price per day or hour
    public int UnitCount { get; set; } // Number of days or hours
    public decimal TotalPrice { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal FinalPrice { get; set; }
    public string Currency { get; set; } = "USD";
    
    // Price breakdown for display
    public List<PriceBreakdownItem>? PriceBreakdown { get; set; }
    
    // Status
    public ReservationStatus Status { get; set; }
    public ReservationSource Source { get; set; } = ReservationSource.Online;
    public string? ConfirmationNumber { get; set; }
    public string? CheckInCode { get; set; }
    public Dictionary<string, string>? SpecialRequests { get; set; } // Multi-language
    
    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? CheckedInAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
    
    // Navigation properties
    public Listing Listing { get; set; } = null!;
    public User Guest { get; set; } = null!;
    public ReservationPayment? Payment { get; set; }
    public Review? Review { get; set; }
}

// Embedded classes for JSONB storage
public class GuestDetails
{
    public int Adults { get; set; }
    public int Children { get; set; }
    public int Seniors { get; set; }
}

public class ReservationSelections
{
    public int Sunbeds { get; set; }
    public int Cabanas { get; set; }
}

public class PriceBreakdownItem
{
    public string Label { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
