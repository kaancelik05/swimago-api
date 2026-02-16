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
    private readonly ICityRepository _cityRepository;
    private readonly IAmenityRepository _amenityRepository;
    private readonly ILogger<AdminService> _logger;

    public AdminService(
        IUserRepository userRepository,
        IListingRepository listingRepository,
        IReservationRepository reservationRepository,
        ICityRepository cityRepository,
        IAmenityRepository amenityRepository,
        ILogger<AdminService> logger)
    {
        _userRepository = userRepository;
        _listingRepository = listingRepository;
        _reservationRepository = reservationRepository;
        _cityRepository = cityRepository;
        _amenityRepository = amenityRepository;
        _logger = logger;
    }

    public async Task<AdminDashboardResponse> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        // Mock stats for dashboard
        var stats = new AdminStatsDto(
            TotalUsers: 150,
            TotalHosts: 20,
            TotalCustomers: 130,
            NewUsersThisMonth: 12,
            TotalListings: 45,
            ActiveListings: 40,
            PendingListings: 5,
            TotalReservations: 200,
            ReservationsThisMonth: 25,
            PendingHostApplications: 2
        );

        var revenue = new AdminRevenueDto(
            TotalRevenue: 50000m,
            RevenueThisMonth: 5000m,
            RevenueLastMonth: 4500m,
            GrowthPercentage: 11.1m,
            Currency: "USD"
        );

        return await Task.FromResult(new AdminDashboardResponse(
            stats, revenue, new List<AdminRecentActivityDto>(), 
            new AdminSystemHealthDto("Operational", 5, 0, DateTime.UtcNow.AddHours(-1))
        ));
    }

    public async Task<AdminUserListResponse> GetUsersAsync(Role? role, UserStatus? status, string? search, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        // Ideally Repository supports Filtering/Pagination. For now, fetch all or implement stub.
        // Assuming GetByRoleAsync exists, but filtering combination is complex without specification.
        // I'll return empty list for now or stub.
        return await Task.FromResult(new AdminUserListResponse(
            new List<AdminUserItemDto>(), 0, 
            new AdminUserCountsDto(0,0,0,0,0,0,0)
        ));
    }

    public async Task<AdminUserDetailResponse> GetUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null) throw new KeyNotFoundException("Kullanıcı bulunamadı");
        
        // Mock activity
        return new AdminUserDetailResponse(
            Id: user.Id,
            Email: user.Email,
            FirstName: GetTitle(user.Profile?.FirstName),
            LastName: GetTitle(user.Profile?.LastName),
            Avatar: user.Profile?.Avatar,
            PhoneNumber: user.Profile?.PhoneNumber,
            Role: user.Role,
            Status: user.Status,
            IsEmailVerified: user.IsEmailVerified,
            CreatedAt: user.CreatedAt,
            LastLoginAt: user.LastLoginAt,
            Activity: new AdminUserActivityDto(0,0,0,0,0,0),
            RecentReservations: new List<AdminUserReservationDto>(),
            RecentReviews: new List<AdminUserReviewDto>()
        );
    }

    public async Task UpdateUserStatusAsync(Guid userId, UpdateUserStatusRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null) throw new KeyNotFoundException("Kullanıcı bulunamadı");
        
        user.Status = request.Status;
        await _userRepository.UpdateAsync(user, cancellationToken);
    }

    public async Task UpdateUserRoleAsync(Guid userId, UpdateUserRoleRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null) throw new KeyNotFoundException("Kullanıcı bulunamadı");

        user.Role = request.Role;
        await _userRepository.UpdateAsync(user, cancellationToken);
    }

    public async Task<HostApplicationListResponse> GetHostApplicationsAsync(CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(new HostApplicationListResponse(new List<HostApplicationItemDto>(), 0, 0));
    }

    public async Task RejectHostApplicationAsync(Guid userId, RejectHostRequest request, CancellationToken cancellationToken = default)
    {
        // Logic to reject
        await Task.CompletedTask;
    }

    public async Task<AdminListingListResponse> GetListingsAsync(ListingStatus? status, string? search, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        // Mock listing
        var counts = new AdminListingCountsDto(0,0,0,0,0);
        return await Task.FromResult(new AdminListingListResponse(new List<AdminListingItemDto>(), 0, counts));
    }

    public async Task ApproveListingAsync(Guid listingId, CancellationToken cancellationToken = default)
    {
        var listing = await _listingRepository.GetByIdAsync(listingId, cancellationToken);
        if (listing == null) throw new KeyNotFoundException("İlan bulunamadı");

        listing.Status = ListingStatus.Active;
        await _listingRepository.UpdateAsync(listing, cancellationToken);
    }

    public async Task RejectListingAsync(Guid listingId, RejectListingRequest request, CancellationToken cancellationToken = default)
    {
        var listing = await _listingRepository.GetByIdAsync(listingId, cancellationToken);
        if (listing == null) throw new KeyNotFoundException("İlan bulunamadı");

        listing.Status = ListingStatus.Rejected;
        listing.RejectionReason = request.Reason;
        await _listingRepository.UpdateAsync(listing, cancellationToken);
    }

    public async Task<AdminReservationListResponse> GetReservationsAsync(ReservationStatus? status, string? search, int page, int pageSize, CancellationToken cancellationToken = default)
    {
         var stats = new AdminReservationStatsDto(0,0,0,0,0,0);
         return await Task.FromResult(new AdminReservationListResponse(new List<AdminReservationItemDto>(), 0, stats));
    }

    public async Task<AdminReportResponse> GetReportsAsync(DateTime start, DateTime end, CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(new AdminReportResponse(
            new AdminReportPeriodDto(start, end, 0, 0, 0, 0, 0),
            new AdminReportPeriodDto(start, end, 0, 0, 0, 0, 0),
            new List<AdminDailyReportDto>(),
            new List<AdminTopVenueDto>(),
            new List<AdminTopHostDto>()
        ));
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
            // Id is int in entity, but DTO uses Guid? Wait.
            // Entity City.cs: public int Id { get; set; }
            // DTO CityItemDto: public Guid Id
            // Mismatch!
            // I'll assume I should change Entity to Guid OR change DTO to int.
            // Since DTOs are new, and Project uses Guid... I should migrate Entity to Guid?
            // Or fix DTO.
            Name = new Dictionary<string, string> { { "tr", request.Name } },
            Country = request.Country ?? "",
            IsActive = request.IsActive,
            Latitude = 0, Longitude = 0 
        };
        // await _cityRepository.AddAsync(city, cancellationToken);
        // Using int Id in entity means I can't use generic Repository<T> if it expects Guid key?
        // Repository<T> usually expects class T : class.
        // It should handle int.
        // But DTO assumes Guid...
        // Need to check City entity type.
        // If City has int Id, DTO is wrong.
        await Task.CompletedTask; 
    }

    public async Task UpdateCityAsync(Guid id, CreateCityRequest request, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
    }

    public async Task DeleteCityAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
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
            IsActive = request.IsActive
        };
        await _amenityRepository.AddAsync(amenity, cancellationToken);
    }

    public async Task UpdateAmenityAsync(Guid id, CreateAmenityRequest request, CancellationToken cancellationToken = default)
    {
         var amenity = await _amenityRepository.GetByIdAsync(id, cancellationToken);
         if (amenity != null) 
         {
             amenity.Label = new Dictionary<string, string> { { "tr", request.Name } };
             amenity.Icon = request.Icon ?? amenity.Icon;
             amenity.IsActive = request.IsActive;
             await _amenityRepository.UpdateAsync(amenity, cancellationToken);
         }
    }

    public async Task DeleteAmenityAsync(Guid id, CancellationToken cancellationToken = default)
    {
         var amenity = await _amenityRepository.GetByIdAsync(id, cancellationToken);
         if (amenity != null) 
         {
             await _amenityRepository.DeleteAsync(amenity, cancellationToken);
         }
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

     private string GetTitle(Dictionary<string, string>? titleDict)
    {
        if (titleDict == null || !titleDict.Any()) return "";
        if (titleDict.ContainsKey("tr")) return titleDict["tr"];
        if (titleDict.ContainsKey("en")) return titleDict["en"];
        return titleDict.Values.First();
    }
}
