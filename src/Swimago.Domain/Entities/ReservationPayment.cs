using Swimago.Domain.Enums;

namespace Swimago.Domain.Entities;

public class ReservationPayment
{
    public Guid Id { get; set; }
    public Guid ReservationId { get; set; }
    public Guid? PaymentMethodId { get; set; }
    
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public PaymentStatus Status { get; set; }
    
    // Payment provider details
    public string? ProviderTransactionId { get; set; }
    public string? PaymentIntentId { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? RefundedAt { get; set; }
    public decimal? RefundAmount { get; set; }
    
    // Navigation property
    public Reservation Reservation { get; set; } = null!;
    public PaymentMethod? PaymentMethod { get; set; }
}
