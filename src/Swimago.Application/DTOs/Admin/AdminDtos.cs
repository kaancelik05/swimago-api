using Swimago.Domain.Enums;

namespace Swimago.Application.DTOs.Admin;

/// <summary>
/// Admin dashboard response
/// </summary>
public record AdminDashboardResponse(
    AdminStatsDto Stats,
    AdminRevenueDto Revenue,
    IEnumerable<AdminRecentActivityDto> RecentActivity,
    AdminSystemHealthDto? SystemHealth
);

/// <summary>
/// Admin statistics
/// </summary>
public record AdminStatsDto(
    int TotalUsers,
    int TotalHosts,
    int TotalCustomers,
    int NewUsersThisMonth,
    int TotalListings,
    int ActiveListings,
    int PendingListings,
    int TotalReservations,
    int ReservationsThisMonth,
    int PendingHostApplications
);

/// <summary>
/// Revenue statistics
/// </summary>
public record AdminRevenueDto(
    decimal TotalRevenue,
    decimal RevenueThisMonth,
    decimal RevenueLastMonth,
    decimal GrowthPercentage,
    string Currency
);

/// <summary>
/// Recent admin activity item
/// </summary>
public record AdminRecentActivityDto(
    string ActivityType,
    string Description,
    DateTime Timestamp,
    Guid? RelatedEntityId,
    string? RelatedEntityType
);

/// <summary>
/// System health info
/// </summary>
public record AdminSystemHealthDto(
    string Status,
    int ActiveSessions,
    int ErrorsLast24h,
    DateTime LastBackup
);

/// <summary>
/// Admin user list response
/// </summary>
public record AdminUserListResponse(
    IEnumerable<AdminUserItemDto> Users,
    int TotalCount,
    AdminUserCountsDto Counts
);

/// <summary>
/// User counts by role/status
/// </summary>
public record AdminUserCountsDto(
    int Total,
    int Admins,
    int Hosts,
    int Customers,
    int Active,
    int Banned,
    int Pending
);

/// <summary>
/// Admin user list item
/// </summary>
public record AdminUserItemDto(
    Guid Id,
    string Email,
    string? FirstName,
    string? LastName,
    string? Avatar,
    Role Role,
    UserStatus Status,
    bool IsEmailVerified,
    int ReservationCount,
    int ReviewCount,
    DateTime CreatedAt,
    DateTime? LastLoginAt
);

/// <summary>
/// Admin user detail response
/// </summary>
public record AdminUserDetailResponse(
    Guid Id,
    string Email,
    string? FirstName,
    string? LastName,
    string? Avatar,
    string? PhoneNumber,
    Role Role,
    UserStatus Status,
    bool IsEmailVerified,
    DateTime CreatedAt,
    DateTime? LastLoginAt,
    AdminUserActivityDto Activity,
    IEnumerable<AdminUserReservationDto>? RecentReservations,
    IEnumerable<AdminUserReviewDto>? RecentReviews
);

/// <summary>
/// User activity summary
/// </summary>
public record AdminUserActivityDto(
    int TotalReservations,
    int CompletedReservations,
    int CancelledReservations,
    int TotalReviews,
    decimal TotalSpent,
    decimal TotalEarned
);

/// <summary>
/// User reservation for admin view
/// </summary>
public record AdminUserReservationDto(
    Guid Id,
    string VenueName,
    DateTime StartTime,
    decimal TotalPrice,
    ReservationStatus Status
);

/// <summary>
/// User review for admin view
/// </summary>
public record AdminUserReviewDto(
    Guid Id,
    string VenueName,
    int Rating,
    string? Comment,
    DateTime CreatedAt
);

/// <summary>
/// Update user status request
/// </summary>
public record UpdateUserStatusRequest(
    UserStatus Status,
    string? Reason
);

/// <summary>
/// Update user role request
/// </summary>
public record UpdateUserRoleRequest(
    Role Role
);

/// <summary>
/// Host applications list response
/// </summary>
public record HostApplicationListResponse(
    IEnumerable<HostApplicationItemDto> Applications,
    int TotalCount,
    int PendingCount
);

/// <summary>
/// Host application item
/// </summary>
public record HostApplicationItemDto(
    Guid UserId,
    string Email,
    string? FirstName,
    string? LastName,
    string? PhoneNumber,
    DateTime ApplicationDate,
    string? BusinessName,
    string? BusinessType,
    string? Description,
    int? ExpectedListings
);

/// <summary>
/// Reject host application request
/// </summary>
public record RejectHostRequest(
    string Reason
);

/// <summary>
/// Admin listings list response
/// </summary>
public record AdminListingListResponse(
    IEnumerable<AdminListingItemDto> Listings,
    int TotalCount,
    AdminListingCountsDto Counts
);

/// <summary>
/// Listing counts by status
/// </summary>
public record AdminListingCountsDto(
    int Total,
    int Active,
    int Pending,
    int Inactive,
    int Rejected
);

/// <summary>
/// Admin listing item
/// </summary>
public record AdminListingItemDto(
    Guid Id,
    string Name,
    string? Slug,
    VenueType VenueType,
    string? ImageUrl,
    string? City,
    Guid HostId,
    string HostName,
    ListingStatus Status,
    decimal BasePricePerDay,
    string Currency,
    decimal Rating,
    int ReviewCount,
    int ReservationCount,
    decimal TotalRevenue,
    DateTime CreatedAt,
    bool IsFeatured
);

/// <summary>
/// Reject listing request
/// </summary>
public record RejectListingRequest(
    string Reason
);

/// <summary>
/// Admin reservations list response
/// </summary>
public record AdminReservationListResponse(
    IEnumerable<AdminReservationItemDto> Reservations,
    int TotalCount,
    AdminReservationStatsDto Stats
);

/// <summary>
/// Reservation statistics
/// </summary>
public record AdminReservationStatsDto(
    int Total,
    int Pending,
    int Confirmed,
    int Completed,
    int Cancelled,
    decimal TotalRevenue
);

/// <summary>
/// Admin reservation item
/// </summary>
public record AdminReservationItemDto(
    Guid Id,
    string ConfirmationNumber,
    string VenueName,
    VenueType VenueType,
    Guid GuestId,
    string GuestName,
    Guid HostId,
    string HostName,
    DateTime StartTime,
    DateTime EndTime,
    decimal TotalPrice,
    string Currency,
    ReservationStatus Status,
    DateTime CreatedAt
);

/// <summary>
/// Admin reports response
/// </summary>
public record AdminReportResponse(
    AdminReportPeriodDto CurrentPeriod,
    AdminReportPeriodDto PreviousPeriod,
    IEnumerable<AdminDailyReportDto> DailyData,
    IEnumerable<AdminTopVenueDto> TopVenues,
    IEnumerable<AdminTopHostDto> TopHosts
);

/// <summary>
/// Report period data
/// </summary>
public record AdminReportPeriodDto(
    DateTime StartDate,
    DateTime EndDate,
    decimal TotalRevenue,
    decimal PlatformFees,
    int TotalReservations,
    int NewUsers,
    int NewListings
);

/// <summary>
/// Daily report data
/// </summary>
public record AdminDailyReportDto(
    DateTime Date,
    decimal Revenue,
    int Reservations,
    int NewUsers
);

/// <summary>
/// Top venue by revenue
/// </summary>
public record AdminTopVenueDto(
    Guid Id,
    string Name,
    decimal Revenue,
    int Reservations,
    decimal Rating
);

/// <summary>
/// Top host by revenue
/// </summary>
public record AdminTopHostDto(
    Guid Id,
    string Name,
    decimal Revenue,
    int ListingCount,
    decimal Rating
);

/// <summary>
/// City management DTOs
/// </summary>
public record CityListResponse(
    IEnumerable<CityItemDto> Cities,
    int TotalCount
);

public record CityItemDto(
    Guid Id,
    string Name,
    string? Country,
    string? Slug,
    int ListingCount,
    bool IsActive
);

public record CreateCityRequest(
    string Name,
    string? Country,
    string? Slug,
    bool IsActive = true
);

/// <summary>
/// Amenity management DTOs
/// </summary>
public record AmenityListResponse(
    IEnumerable<AmenityItemDto> Amenities,
    int TotalCount
);

public record AmenityItemDto(
    Guid Id,
    string Name,
    string? Icon,
    string? Category,
    int UsageCount,
    bool IsActive
);

public record CreateAmenityRequest(
    string Name,
    string? Icon,
    string? Category,
    bool IsActive = true
);

/// <summary>
/// Category management DTOs
/// </summary>
public record CategoryListResponse(
    IEnumerable<CategoryItemDto> Categories,
    int TotalCount
);

public record CategoryItemDto(
    Guid Id,
    string Name,
    string? Slug,
    string? Description,
    int ItemCount,
    bool IsActive
);

public record CreateCategoryRequest(
    string Name,
    string? Slug,
    string? Description,
    bool IsActive = true
);
