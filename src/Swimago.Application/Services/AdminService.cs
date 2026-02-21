using Microsoft.Extensions.Logging;
using Swimago.Application.DTOs.Admin;
using Swimago.Application.Interfaces;
using Swimago.Domain.Entities;
using Swimago.Domain.Enums;
using Swimago.Domain.Interfaces;

namespace Swimago.Application.Services;

public class AdminService : IAdminService
{
    private readonly IUserRepository _userRepository;
    private readonly IListingRepository _listingRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly IReviewRepository _reviewRepository;
    private readonly ICityRepository _cityRepository;
    private readonly IAmenityRepository _amenityRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AdminService> _logger;

    public AdminService(
        IUserRepository userRepository,
        IListingRepository listingRepository,
        IReservationRepository reservationRepository,
        IReviewRepository reviewRepository,
        ICityRepository cityRepository,
        IAmenityRepository amenityRepository,
        IUnitOfWork unitOfWork,
        ILogger<AdminService> logger)
    {
        _userRepository = userRepository;
        _listingRepository = listingRepository;
        _reservationRepository = reservationRepository;
        _reviewRepository = reviewRepository;
        _cityRepository = cityRepository;
        _amenityRepository = amenityRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<AdminDashboardResponse> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var users = (await _userRepository.GetAllAsync(cancellationToken)).ToList();
        var listings = (await _listingRepository.GetAllAsync(cancellationToken)).ToList();
        var reservations = (await _reservationRepository.GetAllAsync(cancellationToken)).ToList();

        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var lastMonthStart = monthStart.AddMonths(-1);

        var totalRevenue = reservations.Where(x => IsRevenueStatus(x.Status)).Sum(x => x.TotalPrice);
        var revenueThisMonth = reservations
            .Where(x => IsRevenueStatus(x.Status))
            .Where(x => x.StartTime >= monthStart)
            .Sum(x => x.TotalPrice);
        var revenueLastMonth = reservations
            .Where(x => IsRevenueStatus(x.Status))
            .Where(x => x.StartTime >= lastMonthStart && x.StartTime < monthStart)
            .Sum(x => x.TotalPrice);

        var pendingHostApplications = users.Count(x => x.Role == Role.Host && x.Status == UserStatus.Pending);
        var pendingListings = listings.Count(x =>
            x.Status == ListingStatus.Pending ||
            x.Status == ListingStatus.Draft ||
            x.Status == ListingStatus.PendingReview);

        var stats = new AdminStatsDto(
            TotalUsers: users.Count,
            TotalHosts: users.Count(x => x.Role == Role.Host),
            TotalCustomers: users.Count(x => x.Role == Role.Customer),
            NewUsersThisMonth: users.Count(x => x.CreatedAt >= monthStart),
            TotalListings: listings.Count,
            ActiveListings: listings.Count(x => x.Status == ListingStatus.Active),
            PendingListings: pendingListings,
            TotalReservations: reservations.Count,
            ReservationsThisMonth: reservations.Count(x => x.CreatedAt >= monthStart),
            PendingHostApplications: pendingHostApplications
        );

        var revenue = new AdminRevenueDto(
            TotalRevenue: totalRevenue,
            RevenueThisMonth: revenueThisMonth,
            RevenueLastMonth: revenueLastMonth,
            GrowthPercentage: ComputeTrendPercent(revenueThisMonth, revenueLastMonth),
            Currency: "USD"
        );

        var recentActivity = BuildRecentActivity(users, listings, reservations);

        return new AdminDashboardResponse(
            stats,
            revenue,
            recentActivity,
            new AdminSystemHealthDto(
                Status: "Operational",
                ActiveSessions: 0,
                ErrorsLast24h: 0,
                LastBackup: now));
    }

    public async Task<AdminUserListResponse> GetUsersAsync(Role? role, UserStatus? status, string? search, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var safePage = Math.Max(1, page);
        var safePageSize = Math.Clamp(pageSize, 1, 100);

        var users = (await _userRepository.GetAllAsync(cancellationToken)).ToList();
        var profiles = await LoadProfilesByUserIdAsync(users.Select(x => x.Id), cancellationToken);
        var reservations = (await _reservationRepository.GetAllAsync(cancellationToken)).ToList();
        var reviews = (await _reviewRepository.GetAllAsync(cancellationToken)).ToList();

        var reservationCountByGuest = reservations
            .GroupBy(x => x.GuestId)
            .ToDictionary(x => x.Key, x => x.Count());
        var reviewCountByGuest = reviews
            .GroupBy(x => x.GuestId)
            .ToDictionary(x => x.Key, x => x.Count());

        var counts = new AdminUserCountsDto(
            Total: users.Count,
            Admins: users.Count(x => x.Role == Role.Admin),
            Hosts: users.Count(x => x.Role == Role.Host),
            Customers: users.Count(x => x.Role == Role.Customer),
            Active: users.Count(x => x.Status == UserStatus.Active),
            Banned: users.Count(x => x.Status == UserStatus.Banned),
            Pending: users.Count(x => x.Status == UserStatus.Pending));

        var filtered = users.AsEnumerable();

        if (role.HasValue)
        {
            filtered = filtered.Where(x => x.Role == role.Value);
        }

        if (status.HasValue)
        {
            filtered = filtered.Where(x => x.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalized = search.Trim();
            filtered = filtered.Where(x =>
            {
                profiles.TryGetValue(x.Id, out var profile);
                var fullName = BuildName(profile);
                return x.Email.Contains(normalized, StringComparison.OrdinalIgnoreCase) ||
                       fullName.Contains(normalized, StringComparison.OrdinalIgnoreCase);
            });
        }

        var filteredList = filtered
            .OrderByDescending(x => x.CreatedAt)
            .ToList();

        var totalCount = filteredList.Count;
        var pagedUsers = filteredList
            .Skip((safePage - 1) * safePageSize)
            .Take(safePageSize)
            .Select(x =>
            {
                profiles.TryGetValue(x.Id, out var profile);
                reservationCountByGuest.TryGetValue(x.Id, out var reservationCount);
                reviewCountByGuest.TryGetValue(x.Id, out var reviewCount);

                var firstName = GetTitle(profile?.FirstName);
                var lastName = GetTitle(profile?.LastName);

                return new AdminUserItemDto(
                    Id: x.Id,
                    Email: x.Email,
                    FirstName: string.IsNullOrWhiteSpace(firstName) ? null : firstName,
                    LastName: string.IsNullOrWhiteSpace(lastName) ? null : lastName,
                    Avatar: profile?.Avatar,
                    Role: x.Role,
                    Status: x.Status,
                    IsEmailVerified: x.IsEmailVerified,
                    ReservationCount: reservationCount,
                    ReviewCount: reviewCount,
                    CreatedAt: x.CreatedAt,
                    LastLoginAt: x.LastLoginAt);
            })
            .ToList();

        return new AdminUserListResponse(pagedUsers, totalCount, counts);
    }

    public async Task<AdminUserDetailResponse> GetUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetWithProfileAsync(userId, cancellationToken);
        if (user == null) throw new KeyNotFoundException("Kullanıcı bulunamadı");

        var userReservations = (await _reservationRepository.GetByGuestIdAsync(userId, cancellationToken))
            .OrderByDescending(x => x.CreatedAt)
            .ToList();
        var userReviews = (await _reviewRepository.FindAsync(x => x.GuestId == userId, cancellationToken))
            .OrderByDescending(x => x.CreatedAt)
            .ToList();

        var listingIds = userReviews.Select(x => x.ListingId).Distinct().ToList();
        var listingNameById = await LoadListingNameMapAsync(listingIds, cancellationToken);
        var hostRevenue = await CalculateHostRevenueAsync(user, cancellationToken);

        var activity = new AdminUserActivityDto(
            TotalReservations: userReservations.Count,
            CompletedReservations: userReservations.Count(x => x.Status == ReservationStatus.Completed),
            CancelledReservations: userReservations.Count(x =>
                x.Status == ReservationStatus.Cancelled || x.Status == ReservationStatus.Rejected),
            TotalReviews: userReviews.Count,
            TotalSpent: userReservations.Where(x => IsRevenueStatus(x.Status)).Sum(x => x.TotalPrice),
            TotalEarned: hostRevenue);

        return new AdminUserDetailResponse(
            Id: user.Id,
            Email: user.Email,
            FirstName: NormalizeOptional(GetTitle(user.Profile?.FirstName)),
            LastName: NormalizeOptional(GetTitle(user.Profile?.LastName)),
            Avatar: user.Profile?.Avatar,
            PhoneNumber: user.Profile?.PhoneNumber,
            Role: user.Role,
            Status: user.Status,
            IsEmailVerified: user.IsEmailVerified,
            CreatedAt: user.CreatedAt,
            LastLoginAt: user.LastLoginAt,
            Activity: activity,
            RecentReservations: userReservations
                .Take(5)
                .Select(x => new AdminUserReservationDto(
                    Id: x.Id,
                    VenueName: GetTitle(x.Listing?.Title),
                    StartTime: x.StartTime,
                    TotalPrice: x.TotalPrice,
                    Status: x.Status))
                .ToList(),
            RecentReviews: userReviews
                .Take(5)
                .Select(x => new AdminUserReviewDto(
                    Id: x.Id,
                    VenueName: listingNameById.GetValueOrDefault(x.ListingId, string.Empty),
                    Rating: x.Rating,
                    Comment: x.Text,
                    CreatedAt: x.CreatedAt))
                .ToList()
        );
    }

    public async Task UpdateUserStatusAsync(Guid userId, UpdateUserStatusRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null) throw new KeyNotFoundException("Kullanıcı bulunamadı");

        user.Status = request.Status;
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateUserRoleAsync(Guid userId, UpdateUserRoleRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null) throw new KeyNotFoundException("Kullanıcı bulunamadı");

        user.Role = request.Role;
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<HostApplicationListResponse> GetHostApplicationsAsync(CancellationToken cancellationToken = default)
    {
        var users = (await _userRepository.GetAllAsync(cancellationToken))
            .Where(x => x.Role == Role.Host && x.Status == UserStatus.Pending)
            .OrderByDescending(x => x.CreatedAt)
            .ToList();
        var profiles = await LoadProfilesByUserIdAsync(users.Select(x => x.Id), cancellationToken);

        var items = users.Select(x =>
        {
            profiles.TryGetValue(x.Id, out var profile);

            return new HostApplicationItemDto(
                UserId: x.Id,
                Email: x.Email,
                FirstName: NormalizeOptional(GetTitle(profile?.FirstName)),
                LastName: NormalizeOptional(GetTitle(profile?.LastName)),
                PhoneNumber: profile?.PhoneNumber,
                ApplicationDate: x.CreatedAt,
                BusinessName: null,
                BusinessType: null,
                Description: null,
                ExpectedListings: null
            );
        }).ToList();

        return new HostApplicationListResponse(items, items.Count, items.Count);
    }

    public async Task RejectHostApplicationAsync(Guid userId, RejectHostRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new KeyNotFoundException("Kullanıcı bulunamadı");
        }

        if (user.Role != Role.Host)
        {
            throw new InvalidOperationException("Host başvurusu bulunamadı");
        }

        user.Role = Role.Customer;
        user.Status = UserStatus.Active;
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Host application rejected for user {UserId}. Reason: {Reason}",
            userId,
            request.Reason);
    }

    public async Task<AdminListingListResponse> GetListingsAsync(ListingStatus? status, string? search, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var safePage = Math.Max(1, page);
        var safePageSize = Math.Clamp(pageSize, 1, 100);

        var allListings = await LoadAllListingsWithAdminIncludesAsync(cancellationToken);
        var filteredBySearch = allListings;

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalized = search.Trim();
            filteredBySearch = filteredBySearch
                .Where(x => MatchesListingSearch(x, normalized))
                .ToList();
        }

        var counts = new AdminListingCountsDto(
            Total: filteredBySearch.Count,
            Active: filteredBySearch.Count(x => NormalizeListingStatus(x.Status) == ListingStatus.Active),
            Pending: filteredBySearch.Count(x => NormalizeListingStatus(x.Status) == ListingStatus.Pending),
            Inactive: filteredBySearch.Count(x => NormalizeListingStatus(x.Status) == ListingStatus.Inactive),
            Rejected: filteredBySearch.Count(x => NormalizeListingStatus(x.Status) == ListingStatus.Rejected));

        var statusFiltered = status.HasValue
            ? filteredBySearch.Where(x => NormalizeListingStatus(x.Status) == status.Value).ToList()
            : filteredBySearch;

        var listingIds = statusFiltered.Select(x => x.Id).ToList();
        var reservations = listingIds.Count == 0
            ? new List<Reservation>()
            : (await _reservationRepository.GetByListingIdsAsync(listingIds, cancellationToken)).ToList();
        var reservationStatsByListing = reservations
            .GroupBy(x => x.ListingId)
            .ToDictionary(
                x => x.Key,
                x => new
                {
                    Count = x.Count(),
                    Revenue = x.Where(r => IsRevenueStatus(r.Status)).Sum(r => r.TotalPrice)
                });

        var items = statusFiltered
            .OrderByDescending(x => x.CreatedAt)
            .Skip((safePage - 1) * safePageSize)
            .Take(safePageSize)
            .Select(x =>
            {
                reservationStatsByListing.TryGetValue(x.Id, out var reservationStats);
                var hostName = BuildName(x.Host?.Profile);
                if (string.IsNullOrWhiteSpace(hostName))
                {
                    hostName = x.Host?.Email ?? string.Empty;
                }

                return new AdminListingItemDto(
                    Id: x.Id,
                    Name: GetTitle(x.Title),
                    Slug: x.Slug,
                    VenueType: MapVenueType(x.Type),
                    ImageUrl: x.Images
                        .OrderByDescending(i => i.IsCover)
                        .ThenBy(i => i.DisplayOrder)
                        .Select(i => i.Url)
                        .FirstOrDefault(),
                    City: x.City,
                    HostId: x.HostId,
                    HostName: hostName,
                    Status: NormalizeListingStatus(x.Status),
                    BasePricePerDay: x.BasePricePerDay,
                    Currency: x.PriceCurrency,
                    Rating: x.Rating,
                    ReviewCount: x.ReviewCount,
                    ReservationCount: reservationStats?.Count ?? 0,
                    TotalRevenue: reservationStats?.Revenue ?? 0m,
                    CreatedAt: x.CreatedAt,
                    IsFeatured: x.IsFeatured);
            })
            .ToList();

        return new AdminListingListResponse(items, statusFiltered.Count, counts);
    }

    public async Task ApproveListingAsync(Guid listingId, CancellationToken cancellationToken = default)
    {
        var listing = await _listingRepository.GetByIdAsync(listingId, cancellationToken);
        if (listing == null) throw new KeyNotFoundException("İlan bulunamadı");

        listing.Status = ListingStatus.Active;
        listing.IsActive = true;
        listing.RejectionReason = null;
        await _listingRepository.UpdateAsync(listing, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task RejectListingAsync(Guid listingId, RejectListingRequest request, CancellationToken cancellationToken = default)
    {
        var listing = await _listingRepository.GetByIdAsync(listingId, cancellationToken);
        if (listing == null) throw new KeyNotFoundException("İlan bulunamadı");

        listing.Status = ListingStatus.Rejected;
        listing.IsActive = false;
        listing.RejectionReason = request.Reason;
        await _listingRepository.UpdateAsync(listing, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<AdminReservationListResponse> GetReservationsAsync(ReservationStatus? status, string? search, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var safePage = Math.Max(1, page);
        var safePageSize = Math.Clamp(pageSize, 1, 100);
        var reservations = (await _reservationRepository.GetAllAsync(cancellationToken)).ToList();

        if (status.HasValue)
        {
            reservations = reservations.Where(x => x.Status == status.Value).ToList();
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalized = search.Trim();
            reservations = reservations.Where(x =>
                (x.ConfirmationNumber ?? string.Empty).Contains(normalized, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        var listingNameById = await LoadListingNameMapAsync(reservations.Select(x => x.ListingId).Distinct(), cancellationToken);
        var listingById = await LoadListingMapAsync(reservations.Select(x => x.ListingId).Distinct(), cancellationToken);
        var guestById = await LoadUsersByIdAsync(reservations.Select(x => x.GuestId).Distinct(), cancellationToken);
        var hostById = await LoadUsersByIdAsync(listingById.Values.Select(x => x.HostId).Distinct(), cancellationToken);

        var items = reservations
            .OrderByDescending(x => x.CreatedAt)
            .Skip((safePage - 1) * safePageSize)
            .Take(safePageSize)
            .Select(x =>
            {
                listingById.TryGetValue(x.ListingId, out var listing);
                guestById.TryGetValue(x.GuestId, out var guest);
                var hostName = string.Empty;
                if (listing != null && hostById.TryGetValue(listing.HostId, out var host))
                {
                    hostName = BuildName(host.Profile);
                    if (string.IsNullOrWhiteSpace(hostName))
                    {
                        hostName = host.Email;
                    }
                }

                return new AdminReservationItemDto(
                    Id: x.Id,
                    ConfirmationNumber: x.ConfirmationNumber ?? string.Empty,
                    VenueName: listingNameById.GetValueOrDefault(x.ListingId, string.Empty),
                    VenueType: x.VenueType,
                    GuestId: x.GuestId,
                    GuestName: guest == null ? string.Empty : BuildName(guest.Profile),
                    HostId: listing?.HostId ?? Guid.Empty,
                    HostName: hostName,
                    StartTime: x.StartTime,
                    EndTime: x.EndTime,
                    TotalPrice: x.TotalPrice,
                    Currency: x.Currency,
                    Status: x.Status,
                    CreatedAt: x.CreatedAt);
            })
            .ToList();

        var stats = new AdminReservationStatsDto(
            Total: reservations.Count,
            Pending: reservations.Count(x => x.Status == ReservationStatus.Pending),
            Confirmed: reservations.Count(x => x.Status == ReservationStatus.Confirmed),
            Completed: reservations.Count(x => x.Status == ReservationStatus.Completed),
            Cancelled: reservations.Count(x => x.Status == ReservationStatus.Cancelled || x.Status == ReservationStatus.Rejected),
            TotalRevenue: reservations.Where(x => IsRevenueStatus(x.Status)).Sum(x => x.TotalPrice));

        return new AdminReservationListResponse(items, reservations.Count, stats);
    }

    public async Task<AdminReportResponse> GetReportsAsync(DateTime start, DateTime end, CancellationToken cancellationToken = default)
    {
        if (end <= start)
        {
            throw new ArgumentException("end, start tarihinden büyük olmalıdır");
        }

        var currentStart = DateTime.SpecifyKind(start, DateTimeKind.Utc);
        var currentEnd = DateTime.SpecifyKind(end, DateTimeKind.Utc);
        var periodLength = currentEnd - currentStart;
        var previousStart = currentStart - periodLength;
        var previousEnd = currentStart;

        var reservations = (await _reservationRepository.GetAllAsync(cancellationToken)).ToList();
        var listings = (await _listingRepository.GetAllAsync(cancellationToken)).ToList();
        var users = (await _userRepository.GetAllAsync(cancellationToken)).ToList();

        var currentReservations = reservations
            .Where(x => x.StartTime >= currentStart && x.StartTime < currentEnd)
            .ToList();
        var previousReservations = reservations
            .Where(x => x.StartTime >= previousStart && x.StartTime < previousEnd)
            .ToList();

        var currentPeriod = new AdminReportPeriodDto(
            StartDate: currentStart,
            EndDate: currentEnd,
            TotalRevenue: currentReservations.Where(x => IsRevenueStatus(x.Status)).Sum(x => x.TotalPrice),
            PlatformFees: currentReservations.Where(x => IsRevenueStatus(x.Status)).Sum(x => x.TotalPrice) * 0.10m,
            TotalReservations: currentReservations.Count,
            NewUsers: users.Count(x => x.CreatedAt >= currentStart && x.CreatedAt < currentEnd),
            NewListings: listings.Count(x => x.CreatedAt >= currentStart && x.CreatedAt < currentEnd));

        var previousPeriod = new AdminReportPeriodDto(
            StartDate: previousStart,
            EndDate: previousEnd,
            TotalRevenue: previousReservations.Where(x => IsRevenueStatus(x.Status)).Sum(x => x.TotalPrice),
            PlatformFees: previousReservations.Where(x => IsRevenueStatus(x.Status)).Sum(x => x.TotalPrice) * 0.10m,
            TotalReservations: previousReservations.Count,
            NewUsers: users.Count(x => x.CreatedAt >= previousStart && x.CreatedAt < previousEnd),
            NewListings: listings.Count(x => x.CreatedAt >= previousStart && x.CreatedAt < previousEnd));

        var dailyData = BuildDailyReport(currentStart, currentEnd, currentReservations, users);
        var listingMap = await LoadListingMapAsync(currentReservations.Select(x => x.ListingId).Distinct(), cancellationToken);
        var listingNameById = listingMap.ToDictionary(x => x.Key, x => GetTitle(x.Value.Title));

        var topVenues = currentReservations
            .GroupBy(x => x.ListingId)
            .Select(group =>
            {
                listingMap.TryGetValue(group.Key, out var listing);
                return new AdminTopVenueDto(
                    Id: group.Key,
                    Name: listingNameById.GetValueOrDefault(group.Key, string.Empty),
                    Revenue: group.Where(x => IsRevenueStatus(x.Status)).Sum(x => x.TotalPrice),
                    Reservations: group.Count(),
                    Rating: listing?.Rating ?? 0m);
            })
            .OrderByDescending(x => x.Revenue)
            .Take(10)
            .ToList();

        var hostIds = listingMap.Values.Select(x => x.HostId).Distinct().ToList();
        var hostById = await LoadUsersByIdAsync(hostIds, cancellationToken);

        var topHosts = currentReservations
            .Where(x => listingMap.ContainsKey(x.ListingId))
            .GroupBy(x => listingMap[x.ListingId].HostId)
            .Select(group =>
            {
                hostById.TryGetValue(group.Key, out var host);
                var hostName = host == null ? string.Empty : BuildName(host.Profile);
                if (string.IsNullOrWhiteSpace(hostName) && host != null)
                {
                    hostName = host.Email;
                }

                var hostListings = listingMap.Values.Where(x => x.HostId == group.Key).ToList();
                var hostRating = hostListings.Count == 0 ? 0m : hostListings.Average(x => x.Rating);

                return new AdminTopHostDto(
                    Id: group.Key,
                    Name: hostName,
                    Revenue: group.Where(x => IsRevenueStatus(x.Status)).Sum(x => x.TotalPrice),
                    ListingCount: hostListings.Count,
                    Rating: hostRating);
            })
            .OrderByDescending(x => x.Revenue)
            .Take(10)
            .ToList();

        return new AdminReportResponse(currentPeriod, previousPeriod, dailyData, topVenues, topHosts);
    }

    // Cities
    public async Task<CityListResponse> GetCitiesAsync(CancellationToken cancellationToken = default)
    {
        var cities = await _cityRepository.GetAllAsync(cancellationToken);
        var items = cities.Select(c => new CityItemDto(
            Id: c.Id,
            Name: GetTitle(c.Name),
            Country: c.Country,
            Slug: null, // Slug missing in entity
            ListingCount: 0,
            IsActive: c.IsActive
        )).ToList();
        return new CityListResponse(items, items.Count);
    }

    public async Task CreateCityAsync(CreateCityRequest request, CancellationToken cancellationToken = default)
    {
        var city = new City
        {
            Id = Guid.NewGuid(),
            Name = new Dictionary<string, string> { { "tr", request.Name } },
            Country = request.Country ?? "",
            IsActive = request.IsActive,
            Latitude = 0,
            Longitude = 0,
            CreatedAt = DateTime.UtcNow
        };
        await _cityRepository.AddAsync(city, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateCityAsync(Guid id, CreateCityRequest request, CancellationToken cancellationToken = default)
    {
        var city = await _cityRepository.GetByIdAsync(id, cancellationToken);
        if (city == null)
        {
            throw new KeyNotFoundException("Şehir bulunamadı");
        }

        city.Name = new Dictionary<string, string> { ["tr"] = request.Name };
        city.Country = request.Country ?? city.Country;
        city.IsActive = request.IsActive;

        await _cityRepository.UpdateAsync(city, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteCityAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var city = await _cityRepository.GetByIdAsync(id, cancellationToken);
        if (city == null)
        {
            throw new KeyNotFoundException("Şehir bulunamadı");
        }

        await _cityRepository.DeleteAsync(city, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    // Amenities
    public async Task<AmenityListResponse> GetAmenitiesAsync(CancellationToken cancellationToken = default)
    {
        var amenities = await _amenityRepository.GetAllAsync(cancellationToken);
        var items = amenities.Select(a => new AmenityItemDto(
            Id: a.Id,
            Name: GetTitle(a.Label),
            Icon: a.Icon,
            Category: a.Category,
            UsageCount: 0,
            IsActive: a.IsActive
        )).ToList();
        return new AmenityListResponse(items, items.Count);
    }

    public async Task CreateAmenityAsync(CreateAmenityRequest request, CancellationToken cancellationToken = default)
    {
        var amenity = new Amenity
        {
            Id = Guid.NewGuid(),
            Label = new Dictionary<string, string> { { "tr", request.Name } },
            Icon = request.Icon ?? "",
            Category = request.Category,
            IsActive = request.IsActive
        };
        await _amenityRepository.AddAsync(amenity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAmenityAsync(Guid id, CreateAmenityRequest request, CancellationToken cancellationToken = default)
    {
        var amenity = await _amenityRepository.GetByIdAsync(id, cancellationToken);
        if (amenity == null)
        {
            throw new KeyNotFoundException("Özellik bulunamadı");
        }

        amenity.Label = new Dictionary<string, string> { { "tr", request.Name } };
        amenity.Icon = request.Icon ?? amenity.Icon;
        amenity.Category = request.Category;
        amenity.IsActive = request.IsActive;

        await _amenityRepository.UpdateAsync(amenity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAmenityAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var amenity = await _amenityRepository.GetByIdAsync(id, cancellationToken);
        if (amenity == null)
        {
            throw new KeyNotFoundException("Özellik bulunamadı");
        }

        await _amenityRepository.DeleteAsync(amenity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    // Categories
    public async Task<CategoryListResponse> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(new CategoryListResponse(new List<CategoryItemDto>(), 0));
    }

    public async Task CreateCategoryAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
    }

    private async Task<Dictionary<Guid, UserProfile?>> LoadProfilesByUserIdAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken)
    {
        var map = new Dictionary<Guid, UserProfile?>();
        foreach (var userId in userIds.Distinct())
        {
            var user = await _userRepository.GetWithProfileAsync(userId, cancellationToken);
            map[userId] = user?.Profile;
        }

        return map;
    }

    private async Task<decimal> CalculateHostRevenueAsync(User user, CancellationToken cancellationToken)
    {
        if (user.Role != Role.Host)
        {
            return 0m;
        }

        var listings = (await _listingRepository.GetByHostIdAsync(user.Id, cancellationToken)).ToList();
        var listingIds = listings.Select(x => x.Id).ToList();
        if (listingIds.Count == 0)
        {
            return 0m;
        }

        var reservations = await _reservationRepository.GetByListingIdsAsync(listingIds, cancellationToken);
        return reservations.Where(x => IsRevenueStatus(x.Status)).Sum(x => x.TotalPrice);
    }

    private async Task<Dictionary<Guid, string>> LoadListingNameMapAsync(IEnumerable<Guid> listingIds, CancellationToken cancellationToken)
    {
        var map = new Dictionary<Guid, string>();
        foreach (var listingId in listingIds.Distinct())
        {
            var listing = await _listingRepository.GetByIdAsync(listingId, cancellationToken);
            if (listing != null)
            {
                map[listingId] = GetTitle(listing.Title);
            }
        }

        return map;
    }

    private async Task<Dictionary<Guid, Listing>> LoadListingMapAsync(IEnumerable<Guid> listingIds, CancellationToken cancellationToken)
    {
        var map = new Dictionary<Guid, Listing>();
        foreach (var listingId in listingIds.Distinct())
        {
            var listing = await _listingRepository.GetByIdAsync(listingId, cancellationToken);
            if (listing != null)
            {
                map[listingId] = listing;
            }
        }

        return map;
    }

    private async Task<Dictionary<Guid, User>> LoadUsersByIdAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken)
    {
        var map = new Dictionary<Guid, User>();
        foreach (var userId in userIds.Distinct())
        {
            var user = await _userRepository.GetWithProfileAsync(userId, cancellationToken);
            if (user != null)
            {
                map[userId] = user;
            }
        }

        return map;
    }

    private async Task<List<Listing>> LoadAllListingsWithAdminIncludesAsync(CancellationToken cancellationToken)
    {
        var statuses = Enum.GetValues<ListingStatus>();
        var allListings = new List<Listing>();

        foreach (var listingStatus in statuses)
        {
            var items = await _listingRepository.GetByStatusAsync(listingStatus, cancellationToken);
            allListings.AddRange(items);
        }

        return allListings
            .GroupBy(x => x.Id)
            .Select(x => x.First())
            .ToList();
    }

    private static List<AdminDailyReportDto> BuildDailyReport(
        DateTime currentStart,
        DateTime currentEnd,
        IReadOnlyCollection<Reservation> currentReservations,
        IReadOnlyCollection<User> users)
    {
        var days = new List<AdminDailyReportDto>();
        var cursor = currentStart.Date;
        while (cursor < currentEnd.Date)
        {
            var next = cursor.AddDays(1);
            var dayRevenue = currentReservations
                .Where(x => x.StartTime >= cursor && x.StartTime < next)
                .Where(x => IsRevenueStatus(x.Status))
                .Sum(x => x.TotalPrice);
            var dayReservations = currentReservations.Count(x => x.StartTime >= cursor && x.StartTime < next);
            var dayUsers = users.Count(x => x.CreatedAt >= cursor && x.CreatedAt < next);

            days.Add(new AdminDailyReportDto(cursor, dayRevenue, dayReservations, dayUsers));
            cursor = next;
        }

        return days;
    }

    private static List<AdminRecentActivityDto> BuildRecentActivity(
        IReadOnlyCollection<User> users,
        IReadOnlyCollection<Listing> listings,
        IReadOnlyCollection<Reservation> reservations)
    {
        var activities = new List<AdminRecentActivityDto>();

        activities.AddRange(users
            .OrderByDescending(x => x.CreatedAt)
            .Take(5)
            .Select(x => new AdminRecentActivityDto(
                ActivityType: "user.created",
                Description: $"New user registered: {x.Email}",
                Timestamp: x.CreatedAt,
                RelatedEntityId: x.Id,
                RelatedEntityType: nameof(User))));

        activities.AddRange(listings
            .OrderByDescending(x => x.CreatedAt)
            .Take(5)
            .Select(x => new AdminRecentActivityDto(
                ActivityType: "listing.created",
                Description: $"Listing created: {GetTitle(x.Title)}",
                Timestamp: x.CreatedAt,
                RelatedEntityId: x.Id,
                RelatedEntityType: nameof(Listing))));

        activities.AddRange(reservations
            .OrderByDescending(x => x.CreatedAt)
            .Take(5)
            .Select(x => new AdminRecentActivityDto(
                ActivityType: "reservation.created",
                Description: $"Reservation created: {x.ConfirmationNumber}",
                Timestamp: x.CreatedAt,
                RelatedEntityId: x.Id,
                RelatedEntityType: nameof(Reservation))));

        return activities
            .OrderByDescending(x => x.Timestamp)
            .Take(10)
            .ToList();
    }

    private static ListingStatus NormalizeListingStatus(ListingStatus listingStatus)
    {
        return listingStatus switch
        {
            ListingStatus.Draft => ListingStatus.Pending,
            ListingStatus.PendingReview => ListingStatus.Pending,
            _ => listingStatus
        };
    }

    private static bool MatchesListingSearch(Listing listing, string search)
    {
        var title = GetTitle(listing.Title);
        var hostName = listing.Host?.Profile == null
            ? string.Empty
            : BuildName(listing.Host.Profile);
        var hostEmail = listing.Host?.Email ?? string.Empty;

        return title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
               (listing.Slug ?? string.Empty).Contains(search, StringComparison.OrdinalIgnoreCase) ||
               (listing.City ?? string.Empty).Contains(search, StringComparison.OrdinalIgnoreCase) ||
               hostName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
               hostEmail.Contains(search, StringComparison.OrdinalIgnoreCase);
    }

    private static VenueType MapVenueType(ListingType listingType)
    {
        return listingType switch
        {
            ListingType.Beach => VenueType.Beach,
            ListingType.Pool => VenueType.Pool,
            ListingType.Yacht => VenueType.Yacht,
            ListingType.DayTrip => VenueType.DayTrip,
            _ => VenueType.Beach
        };
    }

    private static bool IsRevenueStatus(ReservationStatus reservationStatus)
    {
        return reservationStatus == ReservationStatus.Confirmed || reservationStatus == ReservationStatus.Completed;
    }

    private static decimal ComputeTrendPercent(decimal current, decimal previous)
    {
        if (previous == 0)
        {
            return current == 0 ? 0 : 100;
        }

        return decimal.Round(((current - previous) / previous) * 100m, 2);
    }

    private static string BuildName(UserProfile? profile)
    {
        if (profile == null)
        {
            return string.Empty;
        }

        var firstName = GetTitle(profile.FirstName);
        var lastName = GetTitle(profile.LastName);
        return $"{firstName} {lastName}".Trim();
    }

    private static string? NormalizeOptional(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static string GetTitle(Dictionary<string, string>? titleDict)
    {
        if (titleDict == null || !titleDict.Any()) return "";
        if (titleDict.ContainsKey("tr")) return titleDict["tr"];
        if (titleDict.ContainsKey("en")) return titleDict["en"];
        return titleDict.Values.First();
    }
}
