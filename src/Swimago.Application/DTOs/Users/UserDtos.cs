namespace Swimago.Application.DTOs.Users;

/// <summary>
/// Full user profile response
/// </summary>
public record UserProfileResponse(
    Guid Id,
    string Email,
    string Role,
    bool IsEmailVerified,
    DateTime CreatedAt,
    DateTime? LastLoginAt,
    UserProfileDetailsDto Profile,
    UserStatsDto Stats,
    UserSettingsResponseDto Settings
);

/// <summary>
/// User profile details
/// </summary>
public record UserProfileDetailsDto(
    string? FirstName,
    string? LastName,
    string? Avatar,
    string? PhoneNumber,
    string? Bio,
    DateTime? DateOfBirth,
    string? Country,
    string? City
);

/// <summary>
/// User statistics
/// </summary>
public record UserStatsDto(
    int TotalReservations,
    int CompletedReservations,
    int TotalReviews,
    decimal TotalSpent,
    int FavoriteCount,
    string? MembershipLevel
);

/// <summary>
/// User settings response
/// </summary>
public record UserSettingsResponseDto(
    bool EmailNotifications,
    bool SmsNotifications,
    bool PushNotifications,
    string Language,
    string Currency,
    bool ProfilePublic
);

/// <summary>
/// Update user profile request
/// </summary>
public record UpdateProfileRequest(
    string? FirstName,
    string? LastName,
    string? PhoneNumber,
    string? Bio,
    DateTime? DateOfBirth,
    string? Country,
    string? City
);

/// <summary>
/// Update avatar response
/// </summary>
public record UpdateAvatarResponse(
    string AvatarUrl
);

/// <summary>
/// Update user settings request
/// </summary>
public record UpdateSettingsRequest(
    bool? EmailNotifications,
    bool? SmsNotifications,
    bool? PushNotifications,
    string? Language,
    string? Currency,
    bool? ProfilePublic,
    bool? DataSharing
);

/// <summary>
/// Change password request
/// </summary>
public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword,
    string ConfirmPassword
);

/// <summary>
/// Delete account request
/// </summary>
public record DeleteAccountRequest(
    string Password,
    string? Reason
);

public record UserDashboardResponse(
    UserDashboardStatsDto Stats,
    DashboardUpcomingReservationDto? UpcomingReservation,
    IEnumerable<DashboardFavoriteSpotDto> FavoriteSpots
);

public record UserDashboardStatsDto(
    int UpcomingTrips,
    int TotalBookings,
    int RewardPoints,
    int WeeklyDelta
);

public record DashboardUpcomingReservationDto(
    Guid Id,
    string ResortName,
    string Location,
    string? ImageUrl,
    DateTime Date,
    string Time,
    string Guests,
    string Status,
    bool IsFavorite
);

public record DashboardFavoriteSpotDto(
    Guid Id,
    string Title,
    string Location,
    string? ImageUrl,
    decimal Rating,
    string Price,
    string PriceUnit,
    bool IsFavorite
);

public record ChangeEmailRequest(
    string NewEmail,
    string Password
);

public record ChangeEmailResponse(
    string Email,
    string Message
);
