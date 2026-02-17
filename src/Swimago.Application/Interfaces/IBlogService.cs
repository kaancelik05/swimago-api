using Swimago.Application.DTOs.Blog;
using Swimago.Application.DTOs.Common;

namespace Swimago.Application.Interfaces;

public interface IBlogService
{
    Task<BlogPostResponse> CreateBlogPostAsync(Guid authorId, CreateBlogPostRequest request, CancellationToken cancellationToken = default);
    Task<BlogPostResponse> UpdateBlogPostAsync(Guid id, Guid authorId, UpdateBlogPostRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteBlogPostAsync(Guid id, Guid authorId, CancellationToken cancellationToken = default);
    Task<BlogPostResponse?> GetBlogPostByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<BlogPostResponse?> GetBlogPostBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<PagedResult<BlogPostResponse>> GetPublishedPostsAsync(PaginationQuery query, CancellationToken cancellationToken = default);
    Task<IEnumerable<BlogPostResponse>> GetAuthorPostsAsync(Guid authorId, CancellationToken cancellationToken = default);
    Task<bool> PublishPostAsync(Guid id, Guid authorId, CancellationToken cancellationToken = default);
    Task<CustomerBlogDetailResponse?> GetBlogDetailBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<BlogRelatedResponse> GetRelatedPostsBySlugAsync(string slug, int limit = 3, CancellationToken cancellationToken = default);
    Task<BlogCommentListResponse> GetCommentsBySlugAsync(string slug, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<BlogCommentDto> AddCommentBySlugAsync(Guid userId, string slug, CreateBlogCommentRequest request, CancellationToken cancellationToken = default);
}
