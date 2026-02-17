using Swimago.Domain.Enums;

namespace Swimago.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public Role Role { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Active;
    public bool IsEmailVerified { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpiry { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    
    // Membership & Settings
    public string MembershipLevel { get; set; } = "Standard";
    
    // User settings stored as JSONB
    public NotificationSettings NotificationSettings { get; set; } = new();
    public LanguageSettings LanguageSettings { get; set; } = new();
    public PrivacySettings PrivacySettings { get; set; } = new();
    
    // Navigation properties
    public UserProfile? Profile { get; set; }
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    public ICollection<Listing> Listings { get; set; } = new List<Listing>();
    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<PaymentMethod> PaymentMethods { get; set; } = new List<PaymentMethod>();
    public ICollection<BlogComment> BlogComments { get; set; } = new List<BlogComment>();
    public HostBusinessSettings? HostBusinessSettings { get; set; }
}

// Embedded settings classes for JSONB storage
public class NotificationSettings
{
    public bool EmailNotifications { get; set; } = true;
    public bool PushNotifications { get; set; } = true;
}

public class LanguageSettings
{
    public string Language { get; set; } = "tr";
    public string Currency { get; set; } = "USD";
}

public class PrivacySettings
{
    public bool ProfileVisibility { get; set; } = true;
    public bool DataSharing { get; set; } = false;
}
