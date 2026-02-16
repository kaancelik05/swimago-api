using Microsoft.AspNetCore.Http;
using Swimago.Application.DTOs.Admin.Blogs;
using Swimago.Application.DTOs.Admin.Destinations;
using Swimago.Application.DTOs.Admin.Listings;
using Swimago.Application.DTOs.Admin.Media;
using Swimago.Application.DTOs.Admin.Shared;
using Swimago.Domain.Enums;

namespace Swimago.Application.Interfaces;

public interface IAdminDestinationService
{
    Task<PaginatedResponse<DestinationListItemDto>> GetDestinationsAsync(string? search, string? country, bool? isFeatured, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<DestinationDetailDto> GetDestinationAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DestinationDetailDto> CreateDestinationAsync(CreateDestinationRequest request, CancellationToken cancellationToken = default);
    Task<DestinationDetailDto> UpdateDestinationAsync(Guid id, CreateDestinationRequest request, CancellationToken cancellationToken = default);
    Task DeleteDestinationAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IAdminListingService
{
    // Common
    Task<PaginatedResponse<ListingListItemDto>> GetListingsAsync(ListingType type, string? search, string? city, bool? isActive, decimal? minPrice, decimal? maxPrice, int page, int pageSize, CancellationToken cancellationToken = default);
    
    // Beaches
    Task<BeachDetailDto> GetBeachAsync(Guid id, CancellationToken cancellationToken = default);
    Task<BeachDetailDto> CreateBeachAsync(CreateBeachRequest request, CancellationToken cancellationToken = default);
    Task<BeachDetailDto> UpdateBeachAsync(Guid id, CreateBeachRequest request, CancellationToken cancellationToken = default);
    Task DeleteListingAsync(Guid id, CancellationToken cancellationToken = default);

    // Pools
    Task<PoolDetailDto> GetPoolAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PoolDetailDto> CreatePoolAsync(CreatePoolRequest request, CancellationToken cancellationToken = default);
    Task<PoolDetailDto> UpdatePoolAsync(Guid id, CreatePoolRequest request, CancellationToken cancellationToken = default);

    // Yacht Tours
    Task<YachtTourDetailDto> GetYachtTourAsync(Guid id, CancellationToken cancellationToken = default);
    Task<YachtTourDetailDto> CreateYachtTourAsync(CreateYachtTourRequest request, CancellationToken cancellationToken = default);
    Task<YachtTourDetailDto> UpdateYachtTourAsync(Guid id, CreateYachtTourRequest request, CancellationToken cancellationToken = default);

    // Day Trips
    Task<DayTripDetailDto> GetDayTripAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DayTripDetailDto> CreateDayTripAsync(CreateDayTripRequest request, CancellationToken cancellationToken = default);
    Task<DayTripDetailDto> UpdateDayTripAsync(Guid id, CreateDayTripRequest request, CancellationToken cancellationToken = default);
}

public interface IAdminBlogService
{
    Task<PaginatedResponse<BlogListItemDto>> GetBlogsAsync(string? search, string? category, bool? isPublished, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<BlogDetailDto> GetBlogAsync(Guid id, CancellationToken cancellationToken = default);
    Task<BlogDetailDto> CreateBlogAsync(CreateBlogRequest request, CancellationToken cancellationToken = default);
    Task<BlogDetailDto> UpdateBlogAsync(Guid id, CreateBlogRequest request, CancellationToken cancellationToken = default);
    Task DeleteBlogAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IAdminMediaService
{
    Task<MediaUploadResponse> UploadFileAsync(IFormFile file, string folder, CancellationToken cancellationToken = default);
    Task<List<MediaUploadResponse>> UploadFilesAsync(List<IFormFile> files, string folder, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(string fileName, CancellationToken cancellationToken = default);
}
