using Swimago.Domain.Enums;

namespace Swimago.Domain.Entities;

public class PaymentMethod
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Type { get; set; } = "card";
    public PaymentBrand Brand { get; set; }
    public string Last4 { get; set; } = string.Empty;
    public int ExpiryMonth { get; set; }
    public int ExpiryYear { get; set; }
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // External payment provider token
    public string? ProviderToken { get; set; }
    
    // Navigation property
    public User User { get; set; } = null!;
}
