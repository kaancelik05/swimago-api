using Swimago.Application.DTOs.Admin;
using Swimago.Domain.Enums;

namespace Swimago.Application.Interfaces;

public interface IAdminService
{
    Task<AdminDashboardResponse> GetDashboardAsync(CancellationToken cancellationToken = default);
    Task<AdminUserListResponse> GetUsersAsync(Role? role, UserStatus? status, string? search, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<AdminUserDetailResponse> GetUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task UpdateUserStatusAsync(Guid userId, UpdateUserStatusRequest request, CancellationToken cancellationToken = default);
    Task UpdateUserRoleAsync(Guid userId, UpdateUserRoleRequest request, CancellationToken cancellationToken = default);
    Task<HostApplicationListResponse> GetHostApplicationsAsync(CancellationToken cancellationToken = default);
    Task RejectHostApplicationAsync(Guid userId, RejectHostRequest request, CancellationToken cancellationToken = default);
    Task<AdminListingListResponse> GetListingsAsync(ListingStatus? status, string? search, int page, int pageSize, CancellationToken cancellationToken = default);
    Task ApproveListingAsync(Guid listingId, CancellationToken cancellationToken = default);
    Task RejectListingAsync(Guid listingId, RejectListingRequest request, CancellationToken cancellationToken = default);
    Task<AdminReservationListResponse> GetReservationsAsync(ReservationStatus? status, string? search, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<AdminReportResponse> GetReportsAsync(DateTime start, DateTime end, CancellationToken cancellationToken = default);
    
    // Master Data - Cities
    Task<CityListResponse> GetCitiesAsync(CancellationToken cancellationToken = default);
    Task CreateCityAsync(CreateCityRequest request, CancellationToken cancellationToken = default);
    Task UpdateCityAsync(Guid id, CreateCityRequest request, CancellationToken cancellationToken = default);
    Task DeleteCityAsync(Guid id, CancellationToken cancellationToken = default);

    // Master Data - Amenities
    Task<AmenityListResponse> GetAmenitiesAsync(CancellationToken cancellationToken = default);
    Task CreateAmenityAsync(CreateAmenityRequest request, CancellationToken cancellationToken = default);
    Task UpdateAmenityAsync(Guid id, CreateAmenityRequest request, CancellationToken cancellationToken = default);
    Task DeleteAmenityAsync(Guid id, CancellationToken cancellationToken = default);

    // Master Data - Categories (Stubbed)
    Task<CategoryListResponse> GetCategoriesAsync(CancellationToken cancellationToken = default);
    Task CreateCategoryAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default);
}
