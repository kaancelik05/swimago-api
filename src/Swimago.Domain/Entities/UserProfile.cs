namespace Swimago.Domain.Entities;

public class UserProfile
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    
    // Multi-language fields (will be configured as JSONB in EF Core)
    public Dictionary<string, string> FirstName { get; set; } = new();
    public Dictionary<string, string> LastName { get; set; } = new();
    
    public string? PhoneNumber { get; set; }
    public string? Avatar { get; set; } // Profile image URL
    public Dictionary<string, string>? Bio { get; set; }
    
    // Navigation property
    public User User { get; set; } = null!;
}
