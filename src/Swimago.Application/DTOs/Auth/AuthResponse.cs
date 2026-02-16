namespace Swimago.Application.DTOs.Auth;

/// <summary>
/// Authentication response with user details and tokens
/// </summary>
public class AuthResponse
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Avatar { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime TokenExpiry { get; set; }
    public UserSettingsDto? Settings { get; set; }
}

public class UserSettingsDto
{
    public bool EmailNotifications { get; set; }
    public bool SmsNotifications { get; set; }
    public bool PushNotifications { get; set; }
    public string Language { get; set; } = "tr";
    public string Currency { get; set; } = "TRY";
    public bool ProfilePublic { get; set; }
}

/// <summary>
/// Refresh token request
/// </summary>
public record RefreshTokenRequest(
    string RefreshToken
);

/// <summary>
/// Forgot password request - sends reset email
/// </summary>
public record ForgotPasswordRequest(
    string Email
);

/// <summary>
/// Reset password with token
/// </summary>
public record ResetPasswordRequest(
    string Token,
    string NewPassword,
    string ConfirmPassword
);

/// <summary>
/// Logout request - invalidates refresh token
/// </summary>
public record LogoutRequest(
    string? RefreshToken = null
);
