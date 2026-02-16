using Swimago.Domain.Entities;

namespace Swimago.Domain.Interfaces;

public interface IBlogPostRepository : IRepository<BlogPost>
{
    Task<IEnumerable<BlogPost>> GetPublishedPostsAsync(CancellationToken cancellationToken = default);
    Task<BlogPost?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<BlogPost?> GetFeaturedAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<BlogPost>> GetByAuthorIdAsync(Guid authorId, CancellationToken cancellationToken = default);
    Task<IEnumerable<BlogPost>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetCategoriesAsync(CancellationToken cancellationToken = default);
    Task IncrementViewCountAsync(Guid id, CancellationToken cancellationToken = default);
}
